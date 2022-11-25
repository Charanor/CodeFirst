using System.Diagnostics;
using JXS.Assets.Core;
using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace JXS.Graphics.Utils.Assets;

public class TextureAssetLoader : CachedAssetLoader<Texture, TextureAssetDefinition>
{
	public TextureAssetLoader()
	{
		StbImage.stbi_set_flip_vertically_on_load(1);
	}

	public override bool CanLoadAsset(TextureAssetDefinition assetDefinition)
	{
		var path = assetDefinition.Path;
		if (!File.Exists(path))
		{
			return false;
		}

		using var fileStream = File.OpenRead(path);
		var fileInfo = ImageInfo.FromStream(fileStream);
		return fileInfo != null;
	}

	protected override bool IsValidAsset(Texture asset) => !asset.IsDisposed;

	protected override Texture LoadAsset(TextureAssetDefinition definition)
	{
		var (path, mipMapLevels, minFilter, magFilter) = definition;

		using var fileStream = File.OpenRead(path);
		var fileInfo = ImageInfo.FromStream(fileStream);
		Debug.Assert(fileInfo != null);

		var actualInfo = fileInfo.Value;
		var imageResult = ImageResult.FromStream(fileStream, actualInfo.ColorComponents);
		return new Texture2D(imageResult.Data, imageResult.Width, imageResult.Height, mipMapLevels,
			SizedInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte)
		{
			Mipmap = mipMapLevels != 0,
			MinFilter = minFilter,
			MagFilter = magFilter
		};
	}
}