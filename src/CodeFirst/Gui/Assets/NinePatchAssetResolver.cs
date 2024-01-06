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

		// Remove the 1px border around all nine-patches
		var region = new Box2i(minX: 1, minY: 1, texture.Width - 2, texture.Height - 2);

		var (left, right) = GetHorizontalInsets(texture, texture.Height - 1);
		var (top, bottom) = GetVerticalInsets(texture, texX: 0);

		var (paddingLeft, paddingRight) = GetHorizontalInsets(texture, texY: 0);
		var (paddingTop, paddingBottom) = GetVerticalInsets(texture, texture.Width - 1);

		var stretchableArea =
			new Box2i(region.Left + left, region.Top + top, region.Right - right, region.Bottom - bottom);
		var contentPadding = new Box2i(paddingLeft, paddingTop, paddingRight, paddingBottom);
		asset = new NinePatch(new TextureRegion(texture, region), stretchableArea, contentPadding);
		return true;
	}

	private static (int left, int right) GetHorizontalInsets(Texture texture, int texY)
	{
		var left = 0;
		for (var x = 0; x < texture.Width - 2; x++)
		{
			var texX = x + 1;
			var hasPixel = texture.GetRgbaPixel(texX, texY).W != 0;
			if (hasPixel)
			{
				left = x;
				break;
			}
		}

		var right = 0;
		for (var x = 0; x < texture.Width - 2; x++)
		{
			var texX = texture.Width - x - 2;
			var hasPixel = texture.GetRgbaPixel(texX, texY).W != 0;
			if (hasPixel)
			{
				right = x;
				break;
			}
		}

		return (left, right);
	}

	private static (int top, int bottom) GetVerticalInsets(Texture texture, int texX)
	{
		var top = 0;
		for (var y = 0; y < texture.Height - 2; y++)
		{
			var texY = y + 1;
			var hasPixel = texture.GetRgbaPixel(texX, texY).W != 0;
			if (hasPixel)
			{
				top = y;
				break;
			}
		}

		var bottom = 0;
		for (var y = 0; y < texture.Height - 2; y++)
		{
			var texY = texture.Height - y - 2;
			var hasPixel = texture.GetRgbaPixel(texX, texY).W != 0;
			if (hasPixel)
			{
				bottom = y;
				break;
			}
		}

		return (top, bottom);
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