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

	private static readonly uint SpriteVertexCount = (uint)QuadPositions.Length;
	private static readonly uint SpriteIndexCount = (uint)QuadIndices.Length;
	private static readonly int MaxVertexCount = MAX_VERTEX_ELEMENT_COUNT / Vertex.VertexInfo.SizeInBytes;
	private static readonly int MaxSpriteCount = MaxVertexCount / (int)SpriteVertexCount;

	private readonly SpriteBatchShader shader;

	private readonly Vertex[] vertices;
	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;

	private int currentSpriteCount;
	private Texture2D? lastTexture;

	public SpriteBatch()
	{
		shader = new SpriteBatchShader();
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

	public void Dispose()
	{
		shader.Dispose();
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}

	public void Begin(Camera camera)
	{
		shader.ProjectionMatrix = camera.Projection;
		shader.ViewMatrix = camera.View;

		Enable(EnableCap.Blend);
		BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		Disable(EnableCap.DepthTest);
		DepthMask(false);

		shader.Bind();
		vertexArray.Bind();
		lastTexture = null;
	}

	public void End()
	{
		// Flush everything that remains
		Flush();

		vertexArray.Unbind();
		shader.Unbind();
		DepthMask(false);
	}

	public void Draw(Texture2D texture, Vector2 position, Vector2 origin, Vector2 size, float rotation,
		Box2i textureRegion, bool flipX, bool flipY)
	{
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
				new Vector2(u, v)
			);
		}

		currentSpriteCount += 1;
	}

	public void Draw(TextureRegion region, Vector2 position, Vector2 origin, Vector2 size, float rotation, bool flipX,
		bool flipY)
	{
		Draw((Texture2D)region.Texture, position, origin, size, rotation, region.Bounds2D, flipX, flipY);
	}

	public void Draw(Texture2D texture, Vertex[] vertices, int offset, int count)
	{
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

	private void Flush()
	{
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

		shader.Tex = lastTexture;
		DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, offset: 0);
		currentSpriteCount = 0;
	}

	[UsedImplicitly]
	public readonly record struct Vertex(Vector3 Position, Vector2 UVCoordinates)
	{
		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			VertexAttribute.Vector3(VertexAttributeLocation.Position),
			VertexAttribute.Vector2(VertexAttributeLocation.TexCoords)
		);
	}
}