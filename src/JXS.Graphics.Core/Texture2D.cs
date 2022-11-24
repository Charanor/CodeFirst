using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public class Texture2D : Texture
{
	public Texture2D(ReadOnlySpan<byte> data, int width, int height, int mipMapLevels = 1,
		SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8, PixelFormat format = PixelFormat.Rgba,
		PixelType type = PixelType.Float) : base(TextureTarget.Texture2d, data, width, height, depth: 0, mipMapLevels,
		internalFormat, format, type)
	{
		Width = width;
		Height = height;
		Size = new Vector2i(Width, Height);
	}

	public int Width { get; }
	public int Height { get; }
	public Vector2i Size { get; }
}