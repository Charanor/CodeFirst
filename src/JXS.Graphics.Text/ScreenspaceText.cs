using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace JXS.Graphics.Text;

/// <summary>
///     Represents a piece of text that can be drawn in screen space coordinates. Will make efficient use of caching the
///     rendered text for maximum performance.
/// </summary>
public partial class ScreenspaceText : IDisposable
{
	private const float EPSILON = 0.001f;

	private readonly FrameBuffer frameBuffer;

	private Font font;
	private string text;
	private Vector2 size = Vector2.One;

	private Texture? cachedTexture;

	public ScreenspaceText(Font font, string text)
	{
		this.font = font;
		this.text = text;
		frameBuffer = new FrameBuffer();
	}

	/// <summary>
	///     The cached texture from the previous render.
	/// </summary>
	private Texture? CachedTexture
	{
		get => cachedTexture;
		set
		{
			cachedTexture = value;
			if (cachedTexture != null)
			{
				frameBuffer.SetAttachment(FramebufferAttachment.ColorAttachment0, cachedTexture);
			}
			else
			{
				frameBuffer.RemoveAttachment(FramebufferAttachment.ColorAttachment0);
			}
		}
	}

	public string Text
	{
		get => text;
		set
		{
			InvalidateCache();
			text = value;
		}
	}

	public Font Font
	{
		get => font;
		set
		{
			if (font == value)
			{
				return;
			}

			InvalidateCache();
			font = value;
		}
	}

	// Note that changing the position will not invalidate the cache, since that would just move the texture someplace else on screen.
	public Vector2 Position { get; set; }

	public Vector2 Size
	{
		get => size;
		set
		{
			var scale = size / value;
			// If the aspect ratio is 1, that means "value" is a multiple of "size" 
			if (Math.Abs(AspectRatio(scale) - 1) > EPSILON)
			{
				// The new size is not a scaled up/down version of the previous size, we need to re-render the text.
				InvalidateCache();
			}

			size = value;
		}
	}

	public Color4<Rgba> Color { get; set; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		// P.S. We do not want to dispose of the Font object, since we do not own it
		CachedTexture?.Dispose();
		frameBuffer.Dispose();
		InvalidateCache();
	}

	private void InvalidateCache()
	{
		CachedTexture = null;
	}

	private static float AspectRatio(Vector2 vec) => vec.X / vec.Y;
}