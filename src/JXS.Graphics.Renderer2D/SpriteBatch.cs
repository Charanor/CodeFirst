using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace JXS.Graphics.Renderer2D;

public class SpriteBatch : IDisposable
{
	private static readonly IReadOnlyCollection<Vector2> QuadPositions = new[]
	{
		new Vector2(x: 0, y: 0),
		new Vector2(x: 1, y: 0),
		new Vector2(x: 1, y: 1),
		new Vector2(x: 0, y: 1)
	};

	private static readonly IReadOnlyCollection<uint> QuadIndices = new[]
	{
		0u, 2u, 3u,
		0u, 1u, 2u
	};

	private readonly int maxTextureCount; // gl_MaxTextureImageUnits
	private readonly int maxInstanceCount; // min(gl_MaxFragmentUniformVectors, gl_MaxVertexUniformVectors)

	// Unfortunately we can't use a sampler2DArray here, since the textures might be different sizes
	private readonly Texture[] textures;

	private readonly int[] textureIndices;
	private readonly Matrix4[] instanceMatrices;
	private readonly Color4<Rgba>[] colors;
	private readonly Box2[] uvBounds;

	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;

	private int currentTextureIndex;
	private int currentInstanceIndex;

	public SpriteBatch()
	{
		GL.GetInteger(GetPName.MaxTextureImageUnits, ref maxTextureCount);

		var maxFragmentUniformVectors = 0;
		GL.GetInteger(GetPName.MaxFragmentUniformVectors, ref maxFragmentUniformVectors);
		var maxVertexUniformVectors = 0;
		GL.GetInteger(GetPName.MaxVertexUniformVectors, ref maxVertexUniformVectors);
		maxInstanceCount = Math.Min(maxVertexUniformVectors, maxFragmentUniformVectors);

		textures = new Texture[maxTextureCount];
		textureIndices = new int[maxInstanceCount];
		instanceMatrices = new Matrix4[maxInstanceCount];
		colors = new Color4<Rgba>[maxInstanceCount];
		uvBounds = new Box2[maxInstanceCount];

		// NOTE: TexCoord is the same as position for a square.
		var vertices = QuadPositions.Select(position => new Vertex(position, position /*SIC!*/)).ToArray();
		vertexBuffer = new Buffer<Vertex>(vertices, VertexBufferObjectUsage.StaticDraw);
		indexBuffer = new Buffer<uint>(QuadIndices.ToArray(), VertexBufferObjectUsage.StaticDraw);
		vertexArray = VertexArray.CreateForVertexInfo(Vertex.VertexInfo, vertexBuffer, indexBuffer);
		Shader = new DefaultSpriteBatchShaderProgram();
	}

	public bool IsStarted { get; private set; }

	public ShaderProgram Shader { get; set; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
		Shader.Dispose();
	}

	public void Draw(Texture texture, Vector2 position) => Draw(texture, position, texture.Dimensions.Xy);
	public void Draw(Texture texture, Vector2 position, Vector2 size) => Draw(texture, position, size, Color4.White);

	public void Draw(Texture texture, Vector2 position, Vector2 size, Color4<Rgba> tintColor) => Draw(texture, position,
		size, tintColor, Box2.FromSize(Vector2.Zero, texture.Dimensions.Xy));

	public void Draw(Texture texture, Vector2 position, Vector2 size, Color4<Rgba> tintColor, Box2 region)
	{
		if (!IsStarted)
		{
			throw new InvalidOperationException(
				$"Must not call {nameof(Draw)} while not inside a {nameof(Begin)}/{nameof(End)} block");
		}

		size = size == default ? texture.Dimensions.Xy : size;
		tintColor = tintColor == default ? Color4.White : tintColor;
		region = region == default ? Box2.FromSize(Vector2.Zero, texture.Dimensions.Xy) : region;

		var textureIndex = Array.IndexOf(textures, texture);
		if (textureIndex == -1)
		{
			if (currentTextureIndex >= maxTextureCount)
			{
				Flush();
			}

			textureIndex = currentTextureIndex++;
			textures[textureIndex] = texture;
		}

		if (currentInstanceIndex >= maxInstanceCount)
		{
			Flush();
		}

		var transform = Matrix4.CreateTranslation(new Vector3(position)) * Matrix4.CreateScale(new Vector3(size));
		// Must divide by texture size to get UV coordinates
		var uvRegion = new Box2(region.Min / texture.Dimensions.Xy, region.Max / texture.Dimensions.Xy);
		SetInstanceData(textureIndex, transform, tintColor, uvRegion);
		currentInstanceIndex += 1;
	}

	public void Draw(TextureRegion textureRegion, Vector2 position) =>
		Draw(textureRegion, position, textureRegion.Bounds2D.Size);

	public void Draw(TextureRegion textureRegion, Vector2 position, Vector2 size) =>
		Draw(textureRegion, position, size, Color4.White);

	public void Draw(TextureRegion textureRegion, Vector2 position, Vector2 size,
		Color4<Rgba> tintColor) => Draw(textureRegion.Texture, position, size, tintColor, textureRegion.Bounds2D);

	private void SetInstanceData(int textureIndex, Matrix4 transform, Color4<Rgba> color, Box2 uvRegion)
	{
		textureIndices[currentTextureIndex] = textureIndex;
		instanceMatrices[currentInstanceIndex] = transform;
		colors[currentInstanceIndex] = color;
		uvBounds[currentInstanceIndex] = uvRegion;
	}

	public void Begin()
	{
		if (IsStarted)
		{
			throw new InvalidOperationException($"{nameof(SpriteBatch)} is already started.");
		}

		IsStarted = true;
		currentTextureIndex = 0;
		currentInstanceIndex = 0;
	}

	public void End()
	{
		if (!IsStarted)
		{
			throw new InvalidOperationException($"{nameof(SpriteBatch)} is already stopped.");
		}

		Flush();
		IsStarted = false;
	}

	public void Flush()
	{
		if (!IsStarted)
		{
			throw new InvalidOperationException(
				$"Must not call {nameof(Flush)} while not inside a {nameof(Begin)}/{nameof(End)} block");
		}

		if (currentInstanceIndex == 0 || currentTextureIndex == 0)
		{
			return;
		}

		UploadUniforms();
		Shader.Bind();
		vertexArray.Bind();
		GL.DrawElementsInstanced(PrimitiveType.Triangles, QuadIndices.Count, DrawElementsType.UnsignedInt, offset: 0,
			currentInstanceIndex);
		vertexArray.Unbind();
		Shader.Unbind();

		// Reset
		currentTextureIndex = 0;
		currentInstanceIndex = 0;
	}

	private void UploadUniforms()
	{
		if (Shader.TryGetUniformLocation(nameof(textures), out var location))
		{
			var handleArray = new int[textures.Length];
			for (uint i = 0; i < handleArray.Length; i++)
			{
				var texture = textures[i];
				GL.BindTextureUnit(i, texture);
				handleArray[i] = texture.Handle.Handle;
			}

			Shader.SetUniform(location, handleArray);
		}

		if (Shader.TryGetUniformLocation(nameof(textureIndices), out location))
		{
			Shader.SetUniform(location, textureIndices);
		}

		if (Shader.TryGetUniformLocation(nameof(instanceMatrices), out location))
		{
			Shader.SetUniform(location, instanceMatrices);
		}

		if (Shader.TryGetUniformLocation(nameof(colors), out location))
		{
			Shader.SetUniform(location, colors);
		}

		if (Shader.TryGetUniformLocation(nameof(uvBounds), out location))
		{
			Shader.SetUniform(location, uvBounds);
		}
	}

	private readonly record struct Vertex(Vector2 Position, Vector2 UVCoordinates)
	{
		public static readonly VertexAttribute PositionAttribute =
			new(VertexAttributeLocation.Position, ComponentCount: 2, sizeof(float), VertexAttribType.Float,
				Offset: 0);

		public static readonly VertexAttribute TexCoordsAttribute =
			new(VertexAttributeLocation.TexCoords, ComponentCount: 2, sizeof(float), VertexAttribType.Float,
				PositionAttribute.NextAttributeOffset);

		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			PositionAttribute,
			TexCoordsAttribute
		);
	}
}