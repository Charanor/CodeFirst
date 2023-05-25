using System.Diagnostics.CodeAnalysis;
using JXS.Graphics.Core.Exceptions;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core.Utils;

public class MeshBuilder
{
	private readonly IList<Mesh.Vertex> vertices;
	private readonly IList<uint> indices;

	public MeshBuilder()
	{
		vertices = new List<Mesh.Vertex>();
		indices = new List<uint>();
		Material = null;
	}

	public Material? Material { get; private set; }

	public void SetMaterial(Material material) => Material = material;
	public void AddVertex(Mesh.Vertex vertex) => vertices.Add(vertex);
	public void AddIndex(uint index) => indices.Add(index);

	public void AddVertices(IEnumerable<Mesh.Vertex> list)
	{
		foreach (var vertex in list)
		{
			AddVertex(vertex);
		}
	}

	public void AddIndices(IEnumerable<uint> list)
	{
		foreach (var index in list)
		{
			AddIndex(index);
		}
	}

	public Mesh BuildAndClear()
	{
		var mesh = Build();
		Clear();
		return mesh;
	}

	public void Clear()
	{
		vertices.Clear();
		indices.Clear();
		Material = null;
	}

	public Mesh Build()
	{
		if (Material == null)
		{
			throw new InvalidMeshException(
				$"{nameof(MeshBuilder)} requires a material to be assigned before calling {nameof(Build)}()!");
		}

		return new Mesh(vertices, indices, Material);
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public Mesh Cube(float width, float height, float length, Material material)
	{
		Clear();

		// Calculate vertices
		// Bottom vertices
		var x0y0z0 = new Vector3(x: 0, y: 0, z: 0);
		var x1y0z0 = new Vector3(width, y: 0, z: 0);
		var x0y0z1 = new Vector3(x: 0, y: 0, length);
		var x1y0z1 = new Vector3(width, y: 0, length);
		// Top vertices
		var x0y1z0 = new Vector3(x: 0, height, z: 0);
		var x1y1z0 = new Vector3(width, height, z: 0);
		var x0y1z1 = new Vector3(x: 0, height, length);
		var x1y1z1 = new Vector3(width, height, length);

		var backward1 = new Triangle(x0y0z0, x1y0z0, x0y1z0);
		var backward2 = new Triangle(x1y0z0, x1y1z0, x0y1z0);
		var forward1 = new Triangle(x0y0z1, x1y0z1, x0y1z1);
		var forward2 = new Triangle(x1y0z1, x1y1z1, x0y1z1);
		var left1 = new Triangle(x0y0z1, x0y0z0, x0y1z1);
		var left2 = new Triangle(x0y0z0, x0y1z0, x0y1z1);
		var right1 = new Triangle(x1y0z1, x1y0z0, x1y1z1);
		var right2 = new Triangle(x1y0z0, x1y1z0, x1y1z1);
		var bottom1 = new Triangle(x0y0z0, x1y0z0, x1y0z1);
		var bottom2 = new Triangle(x1y0z1, x0y0z1, x0y0z0);
		var top1 = new Triangle(x0y1z0, x1y1z0, x1y1z1);
		var top2 = new Triangle(x1y1z1, x0y1z1, x0y1z0);

		var verts = new Mesh.Vertex[]
		{
			new(backward1.P1, backward1.Normal, Color4.White, Vector2.Zero),
			new(backward1.P2, backward1.Normal, Color4.White, Vector2.Zero),
			new(backward1.P3, backward1.Normal, Color4.White, Vector2.Zero),

			new(backward2.P1, backward2.Normal, Color4.White, Vector2.Zero),
			new(backward2.P2, backward2.Normal, Color4.White, Vector2.Zero),
			new(backward2.P3, backward2.Normal, Color4.White, Vector2.Zero),

			new(forward1.P1, forward1.Normal, Color4.White, Vector2.Zero),
			new(forward1.P2, forward1.Normal, Color4.White, Vector2.Zero),
			new(forward1.P3, forward1.Normal, Color4.White, Vector2.Zero),

			new(forward2.P1, forward2.Normal, Color4.White, Vector2.Zero),
			new(forward2.P2, forward2.Normal, Color4.White, Vector2.Zero),
			new(forward2.P3, forward2.Normal, Color4.White, Vector2.Zero),

			new(left1.P1, left1.Normal, Color4.White, Vector2.Zero),
			new(left1.P2, left1.Normal, Color4.White, Vector2.Zero),
			new(left1.P3, left1.Normal, Color4.White, Vector2.Zero),

			new(left2.P1, left2.Normal, Color4.White, Vector2.Zero),
			new(left2.P2, left2.Normal, Color4.White, Vector2.Zero),
			new(left2.P3, left2.Normal, Color4.White, Vector2.Zero),

			new(right1.P1, right1.Normal, Color4.White, Vector2.Zero),
			new(right1.P2, right1.Normal, Color4.White, Vector2.Zero),
			new(right1.P3, right1.Normal, Color4.White, Vector2.Zero),

			new(right2.P1, right2.Normal, Color4.White, Vector2.Zero),
			new(right2.P2, right2.Normal, Color4.White, Vector2.Zero),
			new(right2.P3, right2.Normal, Color4.White, Vector2.Zero),

			new(bottom1.P1, bottom1.Normal, Color4.White, Vector2.Zero),
			new(bottom1.P2, bottom1.Normal, Color4.White, Vector2.Zero),
			new(bottom1.P3, bottom1.Normal, Color4.White, Vector2.Zero),

			new(bottom2.P1, bottom2.Normal, Color4.White, Vector2.Zero),
			new(bottom2.P2, bottom2.Normal, Color4.White, Vector2.Zero),
			new(bottom2.P3, bottom2.Normal, Color4.White, Vector2.Zero),

			new(top1.P1, top1.Normal, Color4.White, Vector2.Zero),
			new(top1.P2, top1.Normal, Color4.White, Vector2.Zero),
			new(top1.P3, top1.Normal, Color4.White, Vector2.Zero),

			new(top2.P1, top2.Normal, Color4.White, Vector2.Zero),
			new(top2.P2, top2.Normal, Color4.White, Vector2.Zero),
			new(top2.P3, top2.Normal, Color4.White, Vector2.Zero)
		};

		AddVertices(verts);
		AddIndices(verts.Select((_, i) => (uint)i).ToArray());

		SetMaterial(material);
		return BuildAndClear();
	}

	public Mesh Cube(Vector3 extents, Material material) => Cube(extents.X, extents.Y, extents.Z, material);

	private readonly record struct Triangle(Vector3 P1, Vector3 P2, Vector3 P3)
	{
		public Vector3 Normal { get; } = Vector3.Cross(P2 - P1, P3 - P1);
	}
}