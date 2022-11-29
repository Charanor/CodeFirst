namespace JXS.Graphics.Core;

public class Texture2D : Texture
{
	public Texture2D(ReadOnlySpan<byte> data, int width, int height, int mipMapLevels = 1,
		SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8, PixelFormat format = PixelFormat.Rgba,
		PixelType type = PixelType.Float) : base(TextureTarget.Texture2d, data, width, height, depth: 0, mipMapLevels,
		internalFormat, format, type)
	{
	}

	public Texture2D(int width, int height, int mipMapLevels = 1,
		SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8, PixelFormat format = PixelFormat.Rgba,
		PixelType type = PixelType.Float) : this(data: default, width, height, mipMapLevels, internalFormat, format,
		type)
	{
	}
}