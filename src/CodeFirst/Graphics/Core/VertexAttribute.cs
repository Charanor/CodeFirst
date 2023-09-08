namespace CodeFirst.Graphics.Core;

public record VertexAttribute(VertexAttributeLocation Location, int ComponentCount, int ComponentSize,
	VertexAttribType Type, uint Divisor = 0)
{
	public uint Index => (uint)Location;
	public int Size => ComponentCount * ComponentSize;

	public static VertexAttribute Vector2(VertexAttributeLocation location, uint divisor = 0) =>
		new(location, ComponentCount: 2, sizeof(float), VertexAttribType.Float, divisor);

	public static VertexAttribute Vector3(VertexAttributeLocation location, uint divisor = 0) =>
		new(location, ComponentCount: 3, sizeof(float), VertexAttribType.Float, divisor);

	public static VertexAttribute Vector4(VertexAttributeLocation location, uint divisor = 0) =>
		new(location, ComponentCount: 4, sizeof(float), VertexAttribType.Float, divisor);

	public static VertexAttribute Color4(VertexAttributeLocation location, uint divisor = 0) =>
		Vector4(location, divisor);

	public static VertexAttribute Int(VertexAttributeLocation location, uint divisor = 0) =>
		new(location, ComponentCount: 1, sizeof(int), VertexAttribType.Int, divisor);

	public static VertexAttribute[] Matrix4(VertexAttributeLocation location, uint divisor = 0) => new[]
	{
		Vector4(location, divisor),
		Vector4(location + 1, divisor),
		Vector4(location + 2, divisor),
		Vector4(location + 3, divisor)
	};
}