using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public class Mesh : IDisposable
{
	private readonly VertexArray vertexArray;
	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;

	public Mesh(IEnumerable<Vertex> vertices, IEnumerable<uint> indices, Material? material)
	{
		Material = material;

		var vertArray = vertices.ToArray();
		Vertices = vertArray;
		vertexBuffer = new Buffer<Vertex>(vertArray, VertexBufferObjectUsage.StaticDraw);

		var indexArray = indices.ToArray();
		Indices = indexArray;
		indexBuffer = new Buffer<uint>(indexArray, VertexBufferObjectUsage.StaticDraw);

		vertexArray = VertexArray.CreateForVertexInfo(Vertex.VertexInfo, vertexBuffer, indexBuffer);
	}

	public Material? Material { get; }
	public IReadOnlyList<Vertex> Vertices { get; }
	public IReadOnlyList<uint> Indices { get; }
	public PrimitiveType PrimitiveType { get; init; } = PrimitiveType.Triangles;

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}

	public void Draw(Matrix4 model, Matrix4 view, Matrix4 projection)
	{
		if (Material == null)
		{
			// Material-less render.
			vertexArray.Bind();
			{
				DrawElements(PrimitiveType, Indices.Count, DrawElementsType.UnsignedInt, offset: 0);
			}
			vertexArray.Unbind();
			return;
		}
		
		Material.ModelMatrix = model;
		Material.ViewMatrix = view;
		Material.ProjectionMatrix = projection;
		Material.Apply();
		Material.Shader.Bind();
		{
			vertexArray.Bind();
			{
				DrawElements(PrimitiveType, Indices.Count, DrawElementsType.UnsignedInt, offset: 0);
			}
			vertexArray.Unbind();
		}
		Material.Shader.Unbind();
	}

	public readonly record struct Vertex(Vector3 Position, Vector3 Normal, Color4<Rgba> Color, Vector2 TexCoord)
	{
		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			VertexAttribute.Vector3(VertexAttributeLocation.Position),
			VertexAttribute.Vector3(VertexAttributeLocation.Normal),
			VertexAttribute.Color4(VertexAttributeLocation.Color),
			VertexAttribute.Vector2(VertexAttributeLocation.TexCoords)
		);
	}
}