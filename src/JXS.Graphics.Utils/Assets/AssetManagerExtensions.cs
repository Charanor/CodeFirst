using JXS.Assets.Core;
using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace JXS.Graphics.Utils.Assets;

public static class AssetManagerExtensions
{
	static AssetManagerExtensions()
	{
		StbImage.stbi_set_flip_vertically_on_load(1);
	}

	public static Texture LoadTexture(this AssetManager @this, string path, int mipMapLevels = 1,
		TextureMinFilter minFilter = TextureMinFilter.Linear,
		TextureMagFilter magFilter = TextureMagFilter.Linear)
	{
		var rootRelativePath = @this.FromRoot(path);
		var cachedAsset = @this.GetCachedAsset<Texture>(rootRelativePath);
		if (
			cachedAsset != null &&
			!cachedAsset.IsDisposed)
		{
			return cachedAsset;
		}

		using var fileStream = File.OpenRead(rootRelativePath);
		var fileInfo = ImageInfo.FromStream(fileStream);
		if (fileInfo == null)
		{
			throw new NotSupportedException(
				$"{nameof(AssetManager)} does not support loading texture of type {Path.GetExtension(path)}");
		}

		var actualInfo = fileInfo.Value;
		var imageResult = ImageResult.FromStream(fileStream, actualInfo.ColorComponents);
		var newTexture = new Texture2D(imageResult.Data, imageResult.Width, imageResult.Height, mipMapLevels,
			SizedInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte)
		{
			Mipmap = mipMapLevels != 0,
			MinFilter = minFilter,
			MagFilter = magFilter
		};
		@this.CacheAsset(rootRelativePath, newTexture);
		return newTexture;
	}
}