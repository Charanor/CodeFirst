namespace JXS.Graphics.Renderer;

internal class VertexArray : NativeResource
{
	private readonly VertexInfo vertexInfo;
	private readonly VertexArrayHandle handle;

	public VertexArray(VertexInfo vertexInfo)
	{
		this.vertexInfo = vertexInfo;
		handle = GL.CreateVertexArray();
		foreach (var (location, componentCount, _, type, _) in vertexInfo.VertexAttributes)
		{
			var index = (uint)location;
			GL.EnableVertexArrayAttrib(handle, index);
			GL.VertexArrayAttribFormat(handle, index, componentCount, type, normalized: false, relativeoffset: 0);
			GL.VertexArrayAttribBinding(handle, index, index);
		}
	}

	public void LinkBuffers(Buffer<Vertex> vertexBuffer, Buffer<uint> indexBuffer)
	{
		var sizeInBytes = vertexInfo.SizeInBytes;
		foreach (var (location, _, _, _, offset) in vertexInfo.VertexAttributes)
		{
			GL.VertexArrayVertexBuffer(handle, (uint)location, vertexBuffer, new IntPtr(offset), sizeInBytes);
		}

		GL.VertexArrayElementBuffer(handle, indexBuffer);
	}

	public void Bind()
	{
		GL.BindVertexArray(handle);
	}

	/// <summary>
	///     Unbinds ANY current buffer. Does not care if this is actually the current active buffer or not.
	/// </summary>
	public void Unbind()
	{
		GL.BindVertexArray(VertexArrayHandle.Zero);
	}

	protected override void DisposeNativeResources()
	{
		GL.DeleteVertexArray(handle);
	}
}