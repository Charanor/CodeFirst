using System.Collections.Immutable;
using OpenTK.Mathematics;

namespace JXS.Graphics.Renderer;

public class Mesh : NativeResource
{
	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;

	private readonly ImmutableArray<Vertex> vertices;
	private readonly ImmutableArray<uint> indices;

	public Mesh(IEnumerable<Vertex> vertices, IEnumerable<uint> indices, Material material)
	{
		this.vertices = vertices.ToImmutableArray();
		this.indices = indices.ToImmutableArray();
		Material = material;

		vertexBuffer = new Buffer<Vertex>(Vertices.ToArray(), VertexBufferObjectUsage.StaticDraw);
		indexBuffer = new Buffer<uint>(Indices.ToArray(), VertexBufferObjectUsage.StaticDraw);
		vertexArray = new VertexArray(Vertex.VertexInfo);
		vertexArray.LinkBuffers(vertexBuffer, indexBuffer);
	}

	public Material Material { get; }
	public IEnumerable<Vertex> Vertices => vertices;
	public IEnumerable<uint> Indices => indices;

	public void Draw(Matrix4 model, Matrix4 view, Matrix4 projection)
	{
		Material.Bind();

		var shader = Material.ShaderProgram;
		if (shader.TryGetUniform(name: "matrices.model", out var info))
		{
			shader.SetUniform((int)info.Location, model);
		}
		if (shader.TryGetUniform(name: "matrices.view", out  info))
		{
			shader.SetUniform((int)info.Location, view);
		}
		if (shader.TryGetUniform(name: "matrices.projection", out  info))
		{
			shader.SetUniform((int)info.Location, projection);
		}

		vertexArray.Bind();

		GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, offset: 0);

		vertexArray.Unbind();
		Material.Unbind();
	}

	protected override void DisposeNativeResources()
	{
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}
}