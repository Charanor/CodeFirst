using System.Diagnostics;
using System.Text.Json;
using JXS.Assets.Core;
using JXS.Graphics.Core;
using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Assets;

public class FontAssetLoader : CachedAssetLoader<Font, FontAssetDefinition>
{
	private const string FONT_ATLAS_EXTENSION = ".mtsdf";
	private const string JSON_EXT = ".json";

	private readonly AssetManager assetManager;

	public FontAssetLoader(AssetManager assetManager)
	{
		this.assetManager = assetManager;
	}

	protected override Font LoadAsset(FontAssetDefinition definition)
	{
		var path = definition.Path;
		var fontName = Path.GetFileName(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)));

		var fileContents = File.ReadAllText(path);
		Debug.Assert(assetManager.TryLoadAsset(definition.TextureAtlasAsset, out var textureAtlas));

		// For now we only allow JSON, so no need to check the type
		// TODO: Add CSV parser
		return LoadJsonFont(fontName, fileContents, textureAtlas);
	}

	private Font LoadJsonFont(string fontName, string fileContents, Texture textureAtlas)
	{
		// For now we only support MTSDF files
		// TODO: Add support for other file types

		var mtsdfFontFile = JsonSerializer.Deserialize<MtsdfFontFile>(fileContents, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		});
		Debug.Assert(mtsdfFontFile != null);
		Debug.Assert(textureAtlas.Dimensions.Xz == new Vector2i(mtsdfFontFile.Atlas.Width, mtsdfFontFile.Atlas.Height));

		var characterPixelSize = mtsdfFontFile.Atlas.Size;
		var distanceRange = mtsdfFontFile.Atlas.DistanceRange;
		var fontAtlas = new FontAtlas(textureAtlas, characterPixelSize, distanceRange);

		var metrics = (FontMetrics)mtsdfFontFile.Metrics;
		var characterSet = mtsdfFontFile.Glyphs.Select(glyph => new FontGlyph(glyph.Unicode,
			glyph.AtlasBounds?.Position ?? Vector2.Zero, glyph.AtlasBounds?.Size ?? Vector2.Zero, glyph.Advance,
			glyph.PlaneBounds?.Boxed ?? Box2.Empty));
		var kernings = mtsdfFontFile.Kernings.Select(kerning =>
			new FontGlyphKerning(kerning.Unicode1, kerning.Unicode2, kerning.Advance));

		return new Font(fontName, fontAtlas, metrics, characterSet, kernings);
	}

	public override bool CanLoadAsset(FontAssetDefinition assetDefinition)
	{
		var path = assetDefinition.Path;
		var format = Path.GetExtension(path);
		var type = Path.GetExtension(Path.GetFileNameWithoutExtension(path));

		return format.Equals(JSON_EXT, StringComparison.InvariantCultureIgnoreCase) &&
		       type.Equals(FONT_ATLAS_EXTENSION, StringComparison.InvariantCultureIgnoreCase) &&
		       assetManager.CanLoadAsset(assetDefinition.TextureAtlasAsset);
	}

	protected override bool IsValidAsset(Font asset) => !asset.Atlas.Texture.IsDisposed;

	private record MtsdfFontFile(MtsdfFontFile.MtsdfAtlas Atlas, MtsdfFontFile.MtsdfMetrics Metrics,
		IEnumerable<MtsdfFontFile.MtsdfGlyph> Glyphs, IEnumerable<MtsdfFontFile.MtsdfKerning> Kernings)
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