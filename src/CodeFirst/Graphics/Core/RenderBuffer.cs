using CodeFirst.Utils;

namespace CodeFirst.Graphics.Core;

public class RenderBuffer : NativeResource
{
	private readonly RenderbufferHandle handle;

	public RenderBuffer(InternalFormat format, int width, int height)
	{
		handle = CreateRenderbuffer();
		NamedRenderbufferStorage(this, format, width, height);
	}

	protected override void DisposeNativeResources()
	{
		DeleteRenderbuffer(handle);
	}

	public static implicit operator RenderbufferHandle(RenderBuffer renderBuffer) => renderBuffer.handle;
}