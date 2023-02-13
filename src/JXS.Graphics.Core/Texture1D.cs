namespace JXS.Graphics.Core;

public class Texture1D : Texture
{
	public Texture1D(ReadOnlySpan<byte> data, int width, int mipMapLevels = 1,
		SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8, PixelFormat format = PixelFormat.Rgba,
		PixelType type = PixelType.Float) : base(TextureTarget.Texture2d, data, width, height: 0, depth: 0,
		mipMapLevels,
		internalFormat, format, type)
	{
	}
}