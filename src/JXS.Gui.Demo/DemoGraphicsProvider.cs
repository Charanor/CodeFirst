using JXS.Graphics.Core;
using JXS.Graphics.Generated;
using JXS.Graphics.Text;
using JXS.Graphics.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace JXS.Gui.Demo;

public class DemoGraphicsProvider : IGraphicsProvider, IDisposable
{
	private static readonly DemoVertex[] QuadVertices =
	{
		new(new Vector2(x: 0, y: 0)),
		new(new Vector2(x: 1, y: 0)),
		new(new Vector2(x: 1, y: 1)),
		new(new Vector2(x: 0, y: 1))
	};

	private static readonly uint[] QuadIndices =
	{
		0u, 1u, 3u,
		1u, 2u, 3u
	};

	private readonly Camera camera;

	private readonly Buffer<DemoVertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;
	private readonly DemoShader shader;

	public DemoGraphicsProvider(Camera camera)
	{
		this.camera = camera;
		vertexBuffer = new Buffer<DemoVertex>(QuadVertices, VertexBufferObjectUsage.StaticRead);
		indexBuffer = new Buffer<uint>(QuadIndices, VertexBufferObjectUsage.StaticRead);
		vertexArray = VertexArray.CreateForVertexInfo(DemoVertex.VertexInfo, vertexBuffer, indexBuffer);
		shader = new DemoShader();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}

	public void Begin()
	{
		shader.ProjectionMatrix = camera.Projection;
		shader.ViewMatrix = camera.View;

		shader.Bind();
		vertexArray.Bind();
	}

	public void End()
	{
		vertexArray.Unbind();
		shader.Unbind();
	}

	public void DrawRect(Box2 bounds, Color4<Rgba> color)
	{
		shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
		                     Matrix4.CreateTranslation(bounds.X, bounds.Y, z: 0);
		shader.BackgroundColor = color.ToVector4();
		shader.HasTexture = false;
		GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
	}

	public void DrawImage(Box2 bounds, Texture2D texture)
	{
		shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
		                     Matrix4.CreateTranslation(bounds.X, bounds.Y, z: 0);
		shader.HasTexture = true;
		shader.Texture0 = texture;
		GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
	}

	public void DrawText(Font font, int size, string text, Vector2 position, Color4<Rgba> color, float maxTextWidth,
		bool log = false)
	{
		throw new NotSupportedException();
	}

	public Vector2 MeasureText(Font font, int size, string text) => throw new NotSupportedException();

	public readonly record struct DemoVertex(Vector2 Position)
	{
		public static readonly VertexAttribute PositionAttribute =
			new(VertexAttributeLocation.Position, ComponentCount: 2, sizeof(float), VertexAttribType.Float,
				Offset: 0);

		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			PositionAttribute
		);
	}
}