using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public readonly record struct Vertex(Vector3 Position, Vector3 Normal, Color4<Rgba> Color, Vector2 TexCoords)
{
	public static readonly VertexAttribute PositionAttribute =
		new(VertexAttributeLocation.Position, ComponentCount: 3, sizeof(float), VertexAttribType.Float,
			Offset: 0);

	public static readonly VertexAttribute NormalAttribute =
		new(VertexAttributeLocation.Normal, ComponentCount: 3, sizeof(float), VertexAttribType.Float,
			PositionAttribute.NextAttributeOffset);

	public static readonly VertexAttribute ColorAttribute =
		new(VertexAttributeLocation.Color, ComponentCount: 4, sizeof(float), VertexAttribType.Float,
			NormalAttribute.NextAttributeOffset);

	public static readonly VertexAttribute TexCoordsAttribute =
		new(VertexAttributeLocation.TexCoords, ComponentCount: 2, sizeof(float), VertexAttribType.Float,
			ColorAttribute.NextAttributeOffset);

	public static readonly VertexInfo VertexInfo = new(
		typeof(Vertex),
		PositionAttribute,
		NormalAttribute,
		ColorAttribute,
		TexCoordsAttribute
	);
}