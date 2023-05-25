namespace JXS.Graphics.Core;

public record VertexInfo(Type VertexType, params VertexAttribute[] VertexAttributes)
{
	public int SizeInBytes { get; } =
		VertexAttributes.Aggregate(
			seed: 0,
			(total, attribute) => total + attribute.Size
		);
}