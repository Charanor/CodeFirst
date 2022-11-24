namespace JXS.Graphics.Core;

public record VertexAttribute(VertexAttributeLocation Location, int ComponentCount, int ComponentSize, VertexAttribType Type, int Offset)
{
	public uint Index => (uint)Location;
	public int Size => ComponentCount * ComponentSize;
	public int NextAttributeOffset => Offset + Size;
}