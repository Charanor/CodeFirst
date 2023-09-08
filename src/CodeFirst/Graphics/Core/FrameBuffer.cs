using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CodeFirst.Utils;

namespace CodeFirst.Graphics.Core;

public class FrameBuffer : NativeResource
{
	[Flags]
	public enum BindingModeFlag
	{
		Buffer = 1 << 0,
		Viewport = 1 << 1
	}

	private readonly FramebufferHandle handle;

	private readonly IDictionary<FramebufferAttachment, Texture> attachmentTextures;
	private readonly IDictionary<FramebufferAttachment, RenderBuffer> attachmentRenderBuffers;
	private readonly List<NativeResource> managedResources;

	public FrameBuffer()
	{
		handle = CreateFramebuffer();
		attachmentTextures = new Dictionary<FramebufferAttachment, Texture>();
		attachmentRenderBuffers = new Dictionary<FramebufferAttachment, RenderBuffer>();
		managedResources = new List<NativeResource>();
	}

	public FrameBuffer(int width, int height) : this(width, height, FramebufferAttachment.ColorAttachment0,
		FramebufferAttachment.DepthStencilAttachment)
	{
	}

	public FrameBuffer(int width, int height, params FramebufferAttachment[] attachments) : this()
	{
		Width = width;
		Height = height;

		foreach (var attachment in attachments)
		{
			if (!Enum.IsDefined(attachment))
			{
				throw new InvalidEnumArgumentException(nameof(attachments), (int)attachment,
					typeof(FramebufferAttachment));
			}

			SetAttachment(attachment, width, height);
		}
	}

	public int Width { get; }
	public int Height { get; }

	public IEnumerable<Texture> Textures => attachmentTextures.Values;
	public IEnumerable<RenderBuffer> RenderBuffers => attachmentRenderBuffers.Values;

	public void SetAttachment(FramebufferAttachment attachment, int width = 0, int height = 0, int level = 0)
	{
		width = width == 0 ? Width : width;
		height = height == 0 ? Height : height;

		if (width <= 0 || height <= 0)
		{
			throw new InvalidOperationException(
				$"Can not set attachment for {nameof(FrameBuffer)} {handle}; Either give width and height as arguments or use the constructor that takes a width and a height.");
		}

		switch (attachment)
		{
			case FramebufferAttachment.DepthStencilAttachment:
				var depthStencilBuffer = new RenderBuffer(InternalFormat.Depth24Stencil8, width, height);
				SetAttachment(attachment, depthStencilBuffer);
				AddManagedResource(depthStencilBuffer);
				break;
			case FramebufferAttachment.DepthAttachment:
				var depthBuffer = new RenderBuffer(InternalFormat.DepthComponent16, width, height);
				SetAttachment(attachment, depthBuffer);
				AddManagedResource(depthBuffer);
				break;
			case FramebufferAttachment.StencilAttachment:
				var stencilBuffer = new RenderBuffer(InternalFormat.StencilIndex8, width, height);
				SetAttachment(attachment, stencilBuffer);
				AddManagedResource(stencilBuffer);
				break;
			default:
				// It's a color attachment
				var texture = new Texture2D(width, height);
				SetAttachment(attachment, texture, level);
				AddManagedResource(texture);
				break;
		}
	}

	public void SetAttachment(FramebufferAttachment attachment, Texture texture, int level = 0)
	{
		RemoveAttachment(attachment);
		NamedFramebufferTexture(this, attachment, texture, level);
		attachmentTextures.Add(attachment, texture);
	}

	public void SetAttachment(FramebufferAttachment attachment, RenderBuffer renderBuffer)
	{
		RemoveAttachment(attachment);
		NamedFramebufferRenderbuffer(this, attachment, RenderbufferTarget.Renderbuffer, renderBuffer);
		attachmentRenderBuffers.Add(attachment, renderBuffer);
	}

	public void RemoveAttachment(FramebufferAttachment attachment)
	{
		if (attachmentTextures.TryGetValue(attachment, out var texture))
		{
			NamedFramebufferTexture(this, attachment, TextureHandle.Zero, level: 0);
			attachmentTextures.Remove(attachment);
			if (managedResources.Contains(texture))
			{
				texture.Dispose();
			}
		}
		else if (attachmentRenderBuffers.TryGetValue(attachment, out var renderBuffer))
		{
			NamedFramebufferTexture(this, attachment, TextureHandle.Zero, level: 0);
			attachmentRenderBuffers.Remove(attachment);
			if (managedResources.Contains(renderBuffer))
			{
				renderBuffer.Dispose();
			}
		}
	}

	public FrameBufferBinding Binding(FramebufferTarget target = FramebufferTarget.Framebuffer,
		BindingModeFlag modeFlags = BindingModeFlag.Buffer | BindingModeFlag.Viewport) => new(target, modeFlags, this);

	public Texture GetTexture(FramebufferAttachment attachment) => attachmentTextures[attachment];

	public bool TryGetTexture(FramebufferAttachment attachment, [NotNullWhen(true)] out Texture? texture) =>
		attachmentTextures.TryGetValue(attachment, out texture);

	public bool HasAttachment(FramebufferAttachment attachment) => attachmentTextures.ContainsKey(attachment);

	public void AddManagedResource(NativeResource resource) => managedResources.Add(resource);
	public void RemoveManagedResource(NativeResource resource) => managedResources.Remove(resource);

	protected override void DisposeNativeResources()
	{
		DeleteFramebuffer(handle);
	}

	protected override void DisposeManagedResources()
	{
		base.DisposeManagedResources();
		foreach (var managedTexture in managedResources)
		{
			managedTexture.Dispose();
		}
	}

	public static implicit operator FramebufferHandle(FrameBuffer frameBuffer) => frameBuffer.handle;

	public sealed unsafe class FrameBufferBinding : IDisposable
	{
		private readonly FramebufferTarget target;
		private readonly BindingModeFlag[] modeFlags;

		private readonly (int X, int Y, int Width, int Height) previousViewport;

		internal FrameBufferBinding(FramebufferTarget target, BindingModeFlag modeFlags, FrameBuffer frameBuffer)
		{
			this.target = target;
			this.modeFlags = modeFlags.GetFlags().ToArray();

			foreach (var bindingMode in this.modeFlags)
			{
				switch (bindingMode)
				{
					case BindingModeFlag.Buffer:
						BindFramebuffer(target, frameBuffer);
						break;
					case BindingModeFlag.Viewport:
						var viewportValues = new int[4];
						fixed (int* ptr = viewportValues)
						{
							GetIntegerv(GetPName.Viewport, ptr);
						}

						previousViewport = (
							X: viewportValues[0],
							Y: viewportValues[1],
							Width: viewportValues[2],
							Height: viewportValues[3]
						);
						Viewport(x: 0, y: 0, frameBuffer.Width, frameBuffer.Height);
						break;
				}
			}
		}

		public void Dispose()
		{
			foreach (var bindingMode in modeFlags)
			{
				switch (bindingMode)
				{
					case BindingModeFlag.Buffer:
						BindFramebuffer(target, FramebufferHandle.Zero);
						break;
					case BindingModeFlag.Viewport:
						var (x, y, width, height) = previousViewport;
						Viewport(x, y, width, height);
						break;
				}
			}
		}
	}
}