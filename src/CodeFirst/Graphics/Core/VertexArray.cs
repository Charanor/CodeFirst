using CodeFirst.Utils;

namespace CodeFirst.Graphics.Core;

public class VertexArray : NativeResource
{
	private readonly VertexArrayHandle handle;

	public VertexArray()
	{
		handle = CreateVertexArray();
	}

	public void LinkVertexAttribute<TData>(VertexAttribute attribute, Buffer<TData> buffer, int stride, int offset)
		where TData : unmanaged
	{
		var (location, componentCount, _, type, divisor) = attribute;
		var index = (uint)location;
		SetupArrayAttribute(index, componentCount, type, divisor);
		LinkBuffer(buffer, index, offset, stride);
	}

	public void LinkBuffer<TData>(Buffer<TData> buffer, uint location, int offset, int stride)
		where TData : unmanaged => VertexArrayVertexBuffer(handle, location, buffer, new IntPtr(offset), stride);

	public void LinkElementBuffer(Buffer<uint> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void LinkElementBuffer(Buffer<byte> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void LinkElementBuffer(Buffer<ushort> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void SetupArrayAttribute(uint index, int componentCount, VertexAttribType type, uint divisor = 0)
	{
		EnableVertexArrayAttrib(handle, index);
		VertexArrayAttribFormat(handle, index, componentCount, type, normalized: false, relativeoffset: 0);
		VertexArrayAttribBinding(handle, index, index);
		VertexArrayBindingDivisor(handle, index, divisor);
	}

	public void Bind()
	{
		BindVertexArray(handle);
	}

	/// <summary>
	///     Unbinds ANY current buffer. Does not care if this is actually the current active buffer or not.
	/// </summary>
	public void Unbind()
	{
		BindVertexArray(VertexArrayHandle.Zero);
	}

	protected override void DisposeNativeResources()
	{
		DeleteVertexArray(handle);
	}

	public static VertexArray CreateForVertexInfo<TVertex>(VertexInfo vertexInfo, Buffer<TVertex> vertexBuffer,
		Buffer<uint> indexBuffer)
		where TVertex : unmanaged
	{
		if (typeof(TVertex) != vertexInfo.VertexType)
		{
			throw new ArgumentException(
				$"Vertex buffer underlying type (got: {typeof(TVertex)}) must be the same as the {nameof(vertexInfo)} type (got: {vertexInfo.VertexType}).");
		}

		var vertexArray = new VertexArray();

		var sizeInBytes = vertexInfo.SizeInBytes;
		var offset = 0;
		foreach (var attribute in vertexInfo.VertexAttributes)
		{
			vertexArray.LinkVertexAttribute(attribute, vertexBuffer, sizeInBytes, offset);
			offset += attribute.Size;
		}

		vertexArray.LinkElementBuffer(indexBuffer);
		return vertexArray;
	}
}