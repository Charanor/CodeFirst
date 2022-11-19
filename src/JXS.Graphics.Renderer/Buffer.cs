namespace JXS.Graphics.Renderer;

internal class Buffer<TData> : NativeResource where TData:unmanaged
{
	/// <summary>
	///     Creates a new Buffer object and initialises it with some data.
	/// </summary>
	/// <param name="data">the initial data</param>
	/// <param name="usage">how the buffer will be used</param>
	public Buffer(ReadOnlySpan<TData> data, VertexBufferObjectUsage usage)
	{
		Handle = GL.CreateBuffer();
		GL.NamedBufferData(Handle, data, usage);
	}

	/// <summary>
	///     Creates a new Buffer object with <paramref name="reservedSpace" /> of reserved space for future data.
	/// </summary>
	/// <param name="reservedSpace">how much space to reserve on the buffer</param>
	/// <param name="usage">how the buffer will be used</param>
	public Buffer(nint reservedSpace, VertexBufferObjectUsage usage)
	{
		Handle = GL.CreateBuffer();
		GL.NamedBufferData(Handle, reservedSpace, IntPtr.Zero, usage);
	}

	public BufferHandle Handle { get; }

	protected override void DisposeNativeResources()
	{
	}

	public static implicit operator BufferHandle(Buffer<TData> buffer) => buffer.Handle;
}