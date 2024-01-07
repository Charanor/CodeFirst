using System.Diagnostics.CodeAnalysis;
using CodeFirst.Graphics.Core.Utils;
using CodeFirst.Graphics.G2D;
using CodeFirst.Graphics.Generated;
using CodeFirst.Graphics.Text.Layout;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Text;

public class Font : IDisposable
{
	private readonly SortedDictionary<int, FontGlyph> glyphMap;
	private readonly SortedDictionary<int, SortedDictionary<int, float>> kerningMap;

	public Font(string name, FontAtlas atlas, FontMetrics metrics, IEnumerable<FontGlyph> characterSet,
		IEnumerable<FontGlyphKerning> kernings)
	{
		Name = name;
		Atlas = atlas;
		Metrics = metrics;

		glyphMap = new SortedDictionary<int, FontGlyph>();
		foreach (var fontGlyph in characterSet)
		{
			glyphMap.Add(fontGlyph.Code, fontGlyph);
		}

		kerningMap = new SortedDictionary<int, SortedDictionary<int, float>>();
		foreach (var (code, next, advance) in kernings)
		{
			if (!kerningMap.TryGetValue(code, out var subMap))
			{
				subMap = new SortedDictionary<int, float>();
				kerningMap.Add(code, subMap);
			}

			subMap.Add(next, advance);
		}

		Shader = new MtsdfFontShader();
	}

	public string Name { get; }
	public FontAtlas Atlas { get; }
	public FontMetrics Metrics { get; }
	public MtsdfFontShader Shader { get; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Shader.Dispose();
	}

	public float ScalePixelsToEm(float pixelValue) => pixelValue / Atlas.CharacterPixelSize;

	public float ScaleEmToPixelSize(float emValue) => ScaleEmToFontSize(emValue, Atlas.CharacterPixelSize);
	public Vector2 ScaleEmToPixelSize(Vector2 emValue) => ScaleEmToFontSize(emValue, Atlas.CharacterPixelSize);
	public Box2 ScaleEmToPixelSize(Box2 emValue) => ScaleEmToFontSize(emValue, Atlas.CharacterPixelSize);

	public float ScaleEmToFontSize(float emValue, float fontSize) => emValue * fontSize;

	public Vector2 ScaleEmToFontSize(Vector2 emValueVector, float fontSize) => new(
		ScaleEmToFontSize(emValueVector.X, fontSize),
		ScaleEmToFontSize(emValueVector.Y, fontSize)
	);

	public Box2 ScaleEmToFontSize(Box2 emValueBox, float fontSize) => new(
		ScaleEmToFontSize(emValueBox.Min, fontSize),
		ScaleEmToFontSize(emValueBox.Max, fontSize)
	);

	public float ScalePixelsToFontSize(float pixelValue, float fontSize) =>
		ScaleEmToFontSize(ScalePixelsToEm(pixelValue), fontSize);

	public Vector2 ScalePixelsToFontSize(Vector2 pixelValueVector, float fontSize) => new(
		ScalePixelsToFontSize(pixelValueVector.X, fontSize),
		ScalePixelsToFontSize(pixelValueVector.Y, fontSize)
	);

	public Box2 ScalePixelsToFontSize(Box2 pixelValueBox, float fontSize) => new(
		ScalePixelsToFontSize(pixelValueBox.Min, fontSize),
		ScalePixelsToFontSize(pixelValueBox.Max, fontSize)
	);

	public bool SupportsCharacter(int code) => glyphMap.ContainsKey(code);
	public bool SupportsCharacter(char character) => SupportsCharacter(ToCode(character));
	public bool SupportsCharacter(string character) => SupportsCharacter(ToCode(character));
	public bool SupportsCharacter(FontGlyph glyph) => glyphMap.ContainsKey(glyph.Code) && glyphMap.ContainsValue(glyph);

	public bool TryGetGlyph(int code, [NotNullWhen(true)] out FontGlyph? glyph) =>
		glyphMap.TryGetValue(code, out glyph);

	public bool TryGetGlyph(char character, [NotNullWhen(true)] out FontGlyph? glyph) =>
		TryGetGlyph(ToCode(character), out glyph);

	public bool TryGetGlyph(string character, [NotNullWhen(true)] out FontGlyph? glyph) =>
		TryGetGlyph(ToCode(character), out glyph);

	/// <summary>
	///     Gets the kerning value between these two glyphs
	/// </summary>
	/// <param name="glyph">the previous glyph in the sequence</param>
	/// <param name="nextGlyph">the next glyph in the sequence</param>
	/// <returns>the kerning value between the characters, or <c>0</c> if there is no kerning between the characters</returns>
	public float GetKerningBetween(FontGlyph glyph, FontGlyph nextGlyph) =>
		kerningMap.TryGetValue(glyph.Code, out var subMap) && subMap.TryGetValue(nextGlyph.Code, out var kerning)
			? kerning
			: 0;

	/// <summary>
	///     Gets the total value to advance the cursor by between drawing these glyphs.
	/// </summary>
	/// <param name="glyph"></param>
	/// <param name="next"></param>
	/// <returns></returns>
	public float GetTotalAdvanceBetween(FontGlyph glyph, FontGlyph next) =>
		next.Advance + GetKerningBetween(glyph, next);

	public void Draw(SpriteBatch batch, string text, float fontSize, Vector2 position, Color4<Rgba> color,
		Color4<Rgba> backgroundColor = default)
	{
		var layout = new TextLayout(this);
		var textRow = new TextRow(this, layout.TextToGlyphs(text));
		Draw(batch, textRow, fontSize, position, color, backgroundColor);
	}

	public void Draw(SpriteBatch batch, TextRow row, float fontSize, Vector2 position, Color4<Rgba> color,
		Color4<Rgba> backgroundColor = default)
	{
		Shader.BackgroundColor = backgroundColor.ToVector4();
		Shader.ForegroundColor = color.ToVector4();
		Shader.DistanceFieldRange = Atlas.DistanceRange;

		var prevShader = batch.Shader;
		batch.Shader = Shader;
		{
			var virtualCursor =
				position; // - new Vector2(x: 0, font.ScaleEmToFontSize(font.Metrics.Descender, fontSize));
			// virtualCursor.Y = camera.WorldSize.Y - virtualCursor.Y;
			// var yOffset = font.ScalePixelsToFontSize(camera.WorldSize.Y, fontSize) - ;
			FontGlyph? previousGlyph = null;
			foreach (var glyph in row.Glyphs)
			{
				var lineHeight = ScalePixelsToFontSize(row.Size.Y, fontSize);
				virtualCursor = DrawGlyph(batch, fontSize, glyph, previousGlyph, virtualCursor, lineHeight);
				previousGlyph = glyph;
			}
		}
		batch.Shader = prevShader;
	}

	private Vector2 DrawGlyph(SpriteBatch batch, float fontSize, FontGlyph glyph, FontGlyph? previousGlyph,
		Vector2 virtualCursor,
		float lineHeight)
	{
		var kerning = previousGlyph == null ? 0 : GetKerningBetween(previousGlyph, glyph);
		var offset = ScaleEmToFontSize(glyph.Offset, fontSize);
		var location = offset.Location;
		//location.Y = font.ScalePixelsToFontSize(glyph.Size.Y, fontSize) - location.Y;
		location.Y *= -1; // Gotta invert coordinate system 
		location.Y += lineHeight; // Move origin to top left corner instead of bottom left
		var position = virtualCursor + location + new Vector2(kerning, y: 0);

		var camera = batch.Camera;
		batch.Draw(
			Atlas.Texture,
			camera == null ? position : (position.X, camera.WorldSize.Y - position.Y),
			Vector2.Zero,
			offset.Size,
			Color4.White,
			textureRegion: Box2.Round(glyph.AtlasBounds)
		);

		var advance = ScaleEmToFontSize(glyph.Advance, fontSize);
		return virtualCursor + new Vector2(advance, y: 0);
	}

	private static int ToCode(char character) => character;
	private static int ToCode(string character) => char.ConvertToUtf32(character, index: 0);
}