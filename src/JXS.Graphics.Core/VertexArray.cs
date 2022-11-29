namespace JXS.Graphics.Core;

public class VertexArray : NativeResource
{
	private readonly VertexArrayHandle handle;

	public VertexArray()
	{
		handle = CreateVertexArray();
	}

	public void LinkVertexAttribute<TData>(VertexAttribute attribute, Buffer<TData> buffer, int stride)
		where TData : unmanaged
	{
		var (location, componentCount, _, type, offset) = attribute;
		var index = (uint)location;
		SetupArrayAttribute(index, componentCount, type);
		LinkBuffer(buffer, index, offset, stride);
	}

	public void LinkBuffer<TData>(Buffer<TData> buffer, uint location, int offset, int stride)
		where TData : unmanaged => VertexArrayVertexBuffer(handle, location, buffer, new IntPtr(offset), stride);

	public void LinkElementBuffer(Buffer<uint> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void LinkElementBuffer(Buffer<byte> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void LinkElementBuffer(Buffer<ushort> indexBuffer) => VertexArrayElementBuffer(handle, indexBuffer);

	public void SetupArrayAttribute(uint index, int componentCount, VertexAttribType type)
	{
		EnableVertexArrayAttrib(handle, index);
		VertexArrayAttribFormat(handle, index, componentCount, type, normalized: false, relativeoffset: 0);
		VertexArrayAttribBinding(handle, index, index);
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
		var vertexArray = new VertexArray();

		var sizeInBytes = vertexInfo.SizeInBytes;
		foreach (var attribute in vertexInfo.VertexAttributes)
		{
			vertexArray.LinkVertexAttribute(attribute, vertexBuffer, sizeInBytes);
		}

		vertexArray.LinkElementBuffer(indexBuffer);
		return vertexArray;
	}
}