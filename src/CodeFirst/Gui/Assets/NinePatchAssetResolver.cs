using System.Diagnostics.CodeAnalysis;
using CodeFirst.AssetManagement;
using CodeFirst.FileSystem;
using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.Core.Assets;
using CodeFirst.Utils.Logging;
using OpenTK.Mathematics;
using StbImageSharp;

namespace CodeFirst.Gui.Assets;

public class NinePatchAssetResolver : IAssetResolver
{
	private static readonly ILogger Logger = LoggingManager.Get<NinePatchAssetResolver>();

	private readonly TextureAssetResolver textureAssetResolver;

	public NinePatchAssetResolver(TextureAssetDefinition? defaultDefinition = default)
	{
		textureAssetResolver = new TextureAssetResolver(defaultDefinition);
		StbImage.stbi_set_flip_vertically_on_load(1);
	}

	public bool CanLoadAssetOfType(Type type) => type == typeof(NinePatch);

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		var result = TryLoadAsset(fileHandle, out NinePatch? ninePatchAsset);
		asset = ninePatchAsset;
		return result;
	}

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out NinePatch? asset)
	{
		if (!fileHandle.HasExtension(".png") || !fileHandle.FileNameWithoutExtension.EndsWith(".9"))
		{
			asset = default;
			return false;
		}

		if (!textureAssetResolver.TryLoadAsset(fileHandle, out Texture? texture))
		{
			asset = default;
			return false;
		}

		var stretchXStart = 1;
		var stretchXEnd = texture.Width - 1;
		var scanning = false;
		for (var x = 1; x < texture.Width; x++)
		{
			var hasPixel = texture.GetRgbaPixel(x, texture.Height - 1).W != 0;
			if (scanning)
			{
				if (hasPixel)
				{
					continue;
				}

				// Stop scan
				stretchXEnd = x - 1;
				break;
			}

			if (!hasPixel)
			{
				continue;
			}

			scanning = true;
			stretchXStart = x - 1;
		}

		var stretchYStart = 1;
		var stretchYEnd = texture.Height - 1;
		scanning = false;
		for (var y = texture.Height - 1; y > 0; y--)
		{
			var hasPixel = texture.GetRgbaPixel(x: 0, y).W != 0;
			if (scanning)
			{
				if (hasPixel)
				{
					continue;
				}

				// Stop scan
				stretchYEnd = texture.Height - y - 2;
				break;
			}

			if (!hasPixel)
			{
				continue;
			}

			scanning = true;
			stretchYStart = texture.Height - y - 2;
		}

		var stretchableArea = new Box2i(stretchXStart, stretchYStart, stretchXEnd, stretchYEnd);

		var paddingLeft = 0;
		var paddingRight = 0;
		scanning = false;
		for (var x = 1; x < texture.Width; x++)
		{
			var hasPixel = texture.GetRgbaPixel(x, y: 0).W != 0;
			if (scanning)
			{
				if (hasPixel)
				{
					continue;
				}

				// Stop scan
				paddingRight = texture.Width - x - 1;
				break;
			}

			if (!hasPixel)
			{
				continue;
			}

			scanning = true;
			paddingLeft = x - 1;
		}

		var paddingTop = 0;
		var paddingBottom = 0;
		scanning = false;
		for (var y = texture.Height - 1; y > 0; y--)
		{
			var hasPixel = texture.GetRgbaPixel(texture.Width - 1, y).W != 0;
			if (scanning)
			{
				if (hasPixel)
				{
					continue;
				}

				// Stop scan
				paddingBottom = y;
				break;
			}

			if (!hasPixel)
			{
				continue;
			}

			scanning = true;
			paddingTop = texture.Height - y - 2;
		}

		var contentPadding = new Box2i(paddingLeft, paddingTop, paddingRight, paddingBottom);

		// Remove the 1px border around all nine-patches
		var region = Box2i.FromSize((1, 1), (texture.Width - 2, texture.Height - 2));
		asset = new NinePatch(new TextureRegion(texture, region), stretchableArea, contentPadding);
		return true;
	}

	private static PixelFormat ColorComponentsToPixelFormat(ColorComponents components) => components switch
	{
		ColorComponents.Grey => PixelFormat.Red,
		ColorComponents.GreyAlpha => PixelFormat.Rg,
		ColorComponents.RedGreenBlue => PixelFormat.Rgb,
		ColorComponents.RedGreenBlueAlpha => PixelFormat.Rgba,
		_ => PixelFormat.Rgba
	};
}