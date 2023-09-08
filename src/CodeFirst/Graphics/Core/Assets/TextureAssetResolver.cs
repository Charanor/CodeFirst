using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CodeFirst.AssetManagement;
using CodeFirst.FileSystem;
using CodeFirst.Utils;
using CodeFirst.Utils.Logging;
using StbImageSharp;

namespace CodeFirst.Graphics.Core.Assets;

public class TextureAssetResolver : IAssetResolver
{
	private const string ASSET_DEFINITION_NAME = "texture";

	private static readonly ILogger Logger = LoggingManager.Get<TextureAssetResolver>();

	private readonly TextureAssetDefinition defaultDefinition;

	public TextureAssetResolver(TextureAssetDefinition? defaultDefinition = default)
	{
		this.defaultDefinition = defaultDefinition ?? new TextureAssetDefinition();
		StbImage.stbi_set_flip_vertically_on_load(1);
	}

	public bool CanLoadAssetOfType(Type type) => type == typeof(Texture) || type.IsAssignableTo(typeof(Texture));

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		var result = TryLoadAsset(fileHandle, out Texture? textureAsset);
		asset = textureAsset;
		return result;
	}

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out Texture? asset)
	{
		if (!fileHandle.HasExtension(".png", ".jpg"))
		{
			asset = default;
			return false;
		}

		var definition = AssetDefinitionUtils.LoadAssetDefinition<TextureAssetDefinition>(fileHandle.FilePath) ??
		                 defaultDefinition;

		Logger.Trace($"Loading asset {definition}");
		var (mipMapLevels, minFilter, magFilter, wrapS, wrapT) = definition;

		using var fileStream = File.OpenRead(fileHandle.FilePath);
		var fileInfo = ImageInfo.FromStream(fileStream);
		Debug.Assert(fileInfo != null);

		var actualInfo = fileInfo.Value;
		var imageResult = ImageResult.FromStream(fileStream, actualInfo.ColorComponents);

		Logger.Info($"Successfully loaded asset {definition}");
		var pixelFormat = ColorComponentsToPixelFormat(imageResult.Comp);
		Logger.Trace(
			$"{(imageResult.Data.Length, imageResult.Width, imageResult.Height, mipMapLevels, SizedInternalFormat.Rgba8, pixelFormat, PixelType.UnsignedByte)}");
		var createTextureTask = MainThread.Post(() => new Texture2D(imageResult.Data, imageResult.Width,
			imageResult.Height, mipMapLevels,
			SizedInternalFormat.Rgba8, pixelFormat, PixelType.UnsignedByte)
		{
			Mipmap = mipMapLevels != 0,
			MinFilter = minFilter,
			MagFilter = magFilter,
			WrapS = wrapS,
			WrapT = wrapT
		});
		createTextureTask.Wait();
		asset = createTextureTask.Result;
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