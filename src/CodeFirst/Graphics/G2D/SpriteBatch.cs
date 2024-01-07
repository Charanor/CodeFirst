using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.Core.Utils;
using CodeFirst.Graphics.Generated;
using CodeFirst.Utils;
using CodeFirst.Utils.Math;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public sealed class SpriteBatch : IDisposable
{
	// This value is currently arbitrary. There is a bug when too many sprites are rendered, this fixes it.
	private const int MAX_SPRITES = 50;
	private const int MAX_VERTEX_ELEMENT_COUNT = short.MaxValue;

	private static readonly Vector3[] QuadPositions =
	{
		new(x: 0, y: 0, z: 0),
		new(x: 1, y: 0, z: 0),
		new(x: 1, y: 1, z: 0),
		new(x: 0, y: 1, z: 0)
	};

	private static readonly uint[] QuadIndices =
	{
		0u, 2u, 3u,
		0u, 1u, 2u
	};

	private static readonly Texture2D WhiteSquare = new(
		new byte[] { 255, 255, 255, 255 },
		width: 1,
		height: 1,
		format: PixelFormat.Rgba,
		type: PixelType.UnsignedByte
	);

	private static readonly uint SpriteVertexCount = (uint)QuadPositions.Length;
	private static readonly uint SpriteIndexCount = (uint)QuadIndices.Length;
	private static readonly int MaxVertexCount = MAX_VERTEX_ELEMENT_COUNT / Vertex.VertexInfo.SizeInBytes;
	private static readonly int MaxSpriteCount = MaxVertexCount / (int)SpriteVertexCount;

	private readonly ISpriteBatchShader defaultShader;

	private readonly Vertex[] vertices;
	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;

	private ISpriteBatchShader? shader;
	private int currentSpriteCount;
	private Texture2D? lastTexture;

	public SpriteBatch()
	{
		defaultShader = new SpriteBatchShader();
		vertices = new Vertex[MaxVertexCount];

		var indices = new uint[MaxSpriteCount * SpriteIndexCount];
		for (uint index = 0, vertex = 0;
		     index < MaxSpriteCount;
		     index += SpriteIndexCount, vertex += SpriteVertexCount)
		{
			for (var offset = 0; offset < QuadIndices.Length; offset++)
			{
				indices[index + offset] = vertex + QuadIndices[offset];
			}
		}

		indexBuffer = new Buffer<uint>(indices, VertexBufferObjectUsage.DynamicDraw);
		vertexBuffer = new Buffer<Vertex>(vertices, VertexBufferObjectUsage.DynamicDraw);
		vertexArray = VertexArray.CreateForVertexInfo(Vertex.VertexInfo, vertexBuffer, indexBuffer);
	}

	public bool IsStarted { get; private set; }

	public ISpriteBatchShader? Shader
	{
		get => shader;
		set
		{
			if (!IsStarted)
			{
				shader = value;
				return;
			}

			Flush();
			UnbindShader();
			shader = value;
			BindShader();
		}
	}

	public Camera? Camera { get; set; }

	private Matrix4 ProjectionMatrix => Camera?.Projection ?? Matrix4.Identity;
	private Matrix4 ViewMatrix => Camera?.View ?? Matrix4.Identity;

	public void Dispose()
	{
		if (defaultShader is IDisposable disposable)
		{
			disposable.Dispose();
		}

		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}

	public void Begin()
	{
		if (IsStarted)
		{
			return;
		}

		Enable(EnableCap.Blend);
		BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		Disable(EnableCap.DepthTest);
		DepthMask(false);

		BindShader();

		vertexArray.Bind();
		lastTexture = null;
		IsStarted = true;
	}

	public void End()
	{
		if (!IsStarted)
		{
			return;
		}

		// Flush everything that remains
		Flush();

		vertexArray.Unbind();
		UnbindShader();

		DepthMask(false);
		IsStarted = false;
	}

	public void Draw(Texture2D texture, Vector2 position, Vector2 origin, Vector2 size, Color4<Rgba> color,
		float rotation = 0,
		Box2i textureRegion = default, bool flipX = false, bool flipY = false)
	{
		if (!IsStarted)
		{
			DevTools.Throw<SpriteBatch>(
				new InvalidOperationException($"Cannot call {nameof(Draw)} before calling {nameof(Begin)}"));
			return;
		}

		if (lastTexture != texture)
		{
			// Switch texture
			Flush();
			lastTexture = texture;
		}

		if (currentSpriteCount > MAX_SPRITES)
		{
			Flush();
		}

		textureRegion.Width = textureRegion.Width <= 0 ? texture.Width : textureRegion.Width;
		textureRegion.Height = textureRegion.Height <= 0 ? texture.Height : textureRegion.Height;

		var textureSize = (Vector2)texture.Dimensions.Xy;
		var uvBounds = new Box2(textureRegion.Min / textureSize, textureRegion.Max / textureSize).ToVector4();

		for (var i = 0; i < QuadPositions.Length; i++)
		{
			var quadPosition = QuadPositions[i];
			var u = flipX
				? Interpolation.Linear.Apply(uvBounds.Z, uvBounds.X, quadPosition.X)
				: Interpolation.Linear.Apply(uvBounds.X, uvBounds.Z, quadPosition.X);
			var v = flipY
				? Interpolation.Linear.Apply(uvBounds.W, uvBounds.Y, quadPosition.Y)
				: Interpolation.Linear.Apply(uvBounds.Y, uvBounds.W, quadPosition.Y);

			var vertexSize = new Vector2(size.X, size.Y) * quadPosition.Xy;
			var rotatedVertex = Quaternion.FromEulerAngles(pitch: 0, yaw: 0, rotation) * (vertexSize - origin) + origin;
			vertices[currentSpriteCount * SpriteVertexCount + i] = new Vertex(
				new Vector3(position + rotatedVertex, z: -1),
				new Vector2(u, v),
				color
			);
		}

		currentSpriteCount += 1;
	}

	public void Draw(TextureRegion region, Vector2 position, Vector2 origin, Vector2 size, Color4<Rgba> color,
		float rotation = 0,
		bool flipX = false,
		bool flipY = false)
	{
		Draw((Texture2D)region.Texture, position, origin, size, color, rotation, region.Bounds2D, flipX, flipY);
	}

	public void Draw(Texture2D texture, Vertex[] vertices, int offset, int count)
	{
		if (!IsStarted)
		{
			DevTools.Throw<SpriteBatch>(
				new InvalidOperationException($"Cannot call {nameof(Draw)} before calling {nameof(Begin)}"));
			return;
		}

		if (vertices.Length % SpriteVertexCount != 0)
		{
			DevTools.Throw<SpriteBatch>(
				new ArgumentOutOfRangeException($"Vertex array must be a multiple of {SpriteVertexCount}"));
			return;
		}

		var spriteCount = vertices.Length / (int)SpriteVertexCount;
		if (spriteCount > MAX_SPRITES)
		{
			// Too many!
			DevTools.Throw<SpriteBatch>(new ArgumentOutOfRangeException(
				$"Cannot draw more than {MAX_SPRITES} at a time, tried to draw {vertices.Length / SpriteVertexCount}"));
			return;
		}

		Flush();
		lastTexture = texture;
		currentSpriteCount = spriteCount;
		Array.Copy(vertices, offset, this.vertices, destinationIndex: 0, count);
		Flush();
	}

	public void Draw(Color4<Rgba> color, Vector2 position, Vector2 origin, Vector2 size, float rotation = 0)
	{
		Draw(WhiteSquare, position, origin, size, color, rotation);
	}

	public void Draw(IDrawable drawable, Vector2 position, Vector2 size, Color4<Rgba> color = default) =>
		drawable.Draw(this, Box2.FromSize(position, size), color);

	public void Draw(IDrawable drawable, Box2 region, Color4<Rgba> color = default) =>
		drawable.Draw(this, region, color);

	private void Flush()
	{
		if (!IsStarted)
		{
			DevTools.Throw<SpriteBatch>(
				new InvalidOperationException($"Cannot call {nameof(Flush)} before calling {nameof(Begin)}"));
			return;
		}

		if (currentSpriteCount == 0)
		{
			// We haven't drawn anything. This more or less only happens the first time we switch textures.
			return;
		}

		if (lastTexture == null)
		{
			DevTools.Throw<SpriteBatch>(new NullReferenceException("No assigned texture!"));
			return;
		}

		var indexCount = currentSpriteCount * (int)SpriteIndexCount;
		vertexBuffer.SetData(vertices);

		var currentShader = Shader ?? defaultShader;
		currentShader.Texture0 = lastTexture;

		DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, offset: 0);
		currentSpriteCount = 0;
	}

	private void BindShader()
	{
		var currentShader = Shader ?? defaultShader;
		currentShader.ProjectionMatrix = ProjectionMatrix;
		currentShader.ViewMatrix = ViewMatrix;
		currentShader.Bind();
	}

	private void UnbindShader()
	{
		var currentShader = Shader ?? defaultShader;
		currentShader.Unbind();
	}

	public IDisposable SetTemporaryShader(ISpriteBatchShader? tempShader) => new TemporaryShaderHandle(this, tempShader);

	[UsedImplicitly]
	public readonly record struct Vertex(Vector3 Position, Vector2 UVCoordinates, Color4<Rgba> Color)
	{
		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			VertexAttribute.Vector3(VertexAttributeLocation.Position),
			VertexAttribute.Vector2(VertexAttributeLocation.TexCoords),
			VertexAttribute.Color4(VertexAttributeLocation.Color)
		);
	}

	private sealed class TemporaryShaderHandle : IDisposable
	{
		private readonly SpriteBatch spriteBatch;
		private readonly ISpriteBatchShader? prevShader;

		public TemporaryShaderHandle(SpriteBatch spriteBatch, ISpriteBatchShader? shader)
		{
			this.spriteBatch = spriteBatch;
			prevShader = spriteBatch.Shader;
			spriteBatch.Shader = shader;
		}

		public void Dispose()
		{
			spriteBatch.Shader = prevShader;
		}
	}
}