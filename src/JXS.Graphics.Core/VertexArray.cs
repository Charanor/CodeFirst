namespace JXS.Graphics.Core;

public class VertexArray<TVertex> : NativeResource where TVertex : unmanaged
{
	private readonly VertexInfo vertexInfo;
	private readonly VertexArrayHandle handle;

	public VertexArray(VertexInfo vertexInfo)
	{
		this.vertexInfo = vertexInfo;
		handle = CreateVertexArray();
		foreach (var (location, componentCount, _, type, _) in vertexInfo.VertexAttributes)
		{
			var index = (uint)location;
			EnableVertexArrayAttrib(handle, index);
			VertexArrayAttribFormat(handle, index, componentCount, type, normalized: false, relativeoffset: 0);
			VertexArrayAttribBinding(handle, index, index);
		}
	}

	public void LinkBuffers(Buffer<TVertex> vertexBuffer, Buffer<uint> indexBuffer)
	{
		var sizeInBytes = vertexInfo.SizeInBytes;
		foreach (var (location, _, _, _, offset) in vertexInfo.VertexAttributes)
		{
			VertexArrayVertexBuffer(handle, (uint)location, vertexBuffer, new IntPtr(offset), sizeInBytes);
		}

		VertexArrayElementBuffer(handle, indexBuffer);
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
}