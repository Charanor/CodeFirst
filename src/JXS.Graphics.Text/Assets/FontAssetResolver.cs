using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JXS.AssetManagement;
using JXS.FileSystem;
using JXS.Graphics.Core;
using JXS.Graphics.Core.Assets;
using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Assets;

public class FontAssetResolver : IAssetResolver
{
	private readonly TextureAssetResolver textureAssetResolver = new();

	public bool CanLoadAssetOfType(Type type) => type == typeof(Font);

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		var path = fileHandle.FilePath;
		if (!path.EndsWith(".mtsdf.json"))
		{
			asset = default;
			return false;
		}

		var fontName = Path.GetFileName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)));

		var fileContents = File.ReadAllText(path);
		var atlasFileHandle = new FileHandle(Path.ChangeExtension(path, ".png"));
		if (!textureAssetResolver.TryLoadAsset(atlasFileHandle, out Texture? textureAtlas))
		{
			asset = default;
			return false;
		}

		var atlas2D = textureAtlas as Texture2D;
		Debug.Assert(atlas2D != null);

		asset = LoadJsonFont(fontName, fileContents, atlas2D);
		return true;
	}

	private Font LoadJsonFont(string fontName, string fileContents, Texture2D textureAtlas)
	{
		var mtsdfFontFile = JsonSerializer.Deserialize<MtsdfFontFile>(fileContents, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		});
		Debug.Assert(mtsdfFontFile != null);
		Debug.Assert(textureAtlas.Dimensions.Xy == new Vector2i(mtsdfFontFile.Atlas.Width, mtsdfFontFile.Atlas.Height));

		var characterPixelSize = mtsdfFontFile.Atlas.Size;
		var distanceRange = mtsdfFontFile.Atlas.DistanceRange;
		var fontAtlas = new FontAtlas(textureAtlas, characterPixelSize, distanceRange);

		var metrics = (FontMetrics)mtsdfFontFile.Metrics;
		var characterSet = mtsdfFontFile.Glyphs.Select(glyph => new FontGlyph(glyph.Unicode,
			glyph.AtlasBounds?.Position ?? Vector2.Zero, glyph.AtlasBounds?.Size ?? Vector2.Zero, glyph.Advance,
			glyph.PlaneBounds?.Boxed ?? Box2.Empty));
		var kernings = mtsdfFontFile.Kerning.Select(kerning =>
			new FontGlyphKerning(kerning.Unicode1, kerning.Unicode2, kerning.Advance));

		return new Font(fontName, fontAtlas, metrics, characterSet, kernings);
	}

	private record MtsdfFontFile(MtsdfFontFile.MtsdfAtlas Atlas, MtsdfFontFile.MtsdfMetrics Metrics,
		IEnumerable<MtsdfFontFile.MtsdfGlyph> Glyphs, IEnumerable<MtsdfFontFile.MtsdfKerning> Kerning)
	{
		public record MtsdfAtlas(string Type, int DistanceRange, float Size, int Width, int Height, string YOrigin);

		public record MtsdfMetrics(float EmSize, float LineHeight, float Ascender, float Descender, float UnderlineY,
			float UnderlineThickness)
		{
			public static explicit operator FontMetrics(MtsdfMetrics @this) => new(@this.EmSize, @this.LineHeight,
				@this.Ascender, @this.Descender, @this.UnderlineY, @this.UnderlineThickness);
		}

		public record MtsdfGlyph(int Unicode, float Advance, Bounds? PlaneBounds, Bounds? AtlasBounds);

		public record MtsdfKerning(int Unicode1, int Unicode2, float Advance);

		public record Bounds(float Left, float Bottom, float Right, float Top)
		{
			public Vector2 Position => Boxed.Location;
			public Vector2 Size => Boxed.Size;
			public Box2 Boxed => Box2.FromPositions(Left, Bottom, Right, Top);
		}
	}
}