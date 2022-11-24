namespace JXS.Graphics.Core;

public class Texture3D : Texture
{
	public Texture3D(ReadOnlySpan<byte> data, int width, int height, int depth, int mipMapLevels = 1,
		SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8, PixelFormat format = PixelFormat.Rgba,
		PixelType type = PixelType.Float) : base(TextureTarget.Texture3d, data, width, height, depth, mipMapLevels,
		internalFormat, format, type)
	{
	}
}