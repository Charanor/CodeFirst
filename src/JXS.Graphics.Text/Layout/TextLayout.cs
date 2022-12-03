using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Layout;

public class TextLayout
{
	// When we can't render a character, try to render these characters in order instead
	private static readonly char[] DefaultFallbackCharacters =
	{
		'⍰',
		'?',
		'.',
		' '
	};

	private readonly Font font;
	private readonly char[] fallbackCharacters;

	public TextLayout(Font font, IEnumerable<char>? fallbackCharacters = default)
	{
		this.font = font;
		this.fallbackCharacters = fallbackCharacters?.ToArray() ?? DefaultFallbackCharacters;
	}

	public IEnumerable<FontGlyph> TextToGlyphs(string text)
	{
		return text.Select(character =>
		{
			if (font.TryGetGlyph(character, out var glyph))
			{
				return glyph;
			}

			return fallbackCharacters.Any(fallbackCharacter => font.TryGetGlyph(fallbackCharacter, out glyph))
				? glyph
				: null; // No fallback characters worked. Just don't do anything with this glyph I guess?
		}).Where(glyph => glyph != null)!;
	}

	public IEnumerable<TextRow> LineBreak(string text, float maxWidth,
		TextBreakStrategy strategy = TextBreakStrategy.Whitespace) => LineBreak(TextToGlyphs(text), maxWidth, strategy);

	public IEnumerable<TextRow> LineBreak(IEnumerable<FontGlyph> fontGlyphs, float maxWidth,
		TextBreakStrategy strategy = TextBreakStrategy.Whitespace)
	{
		if (strategy == TextBreakStrategy.None)
		{
			// We can't linebreak so just return all the glyphs
			return new[] { new TextRow(font, fontGlyphs) };
		}

		var newRows = new List<TextRow>();

		var previousRowEndIndex = 0;
		var lastValidLineBreakIndex = 0;
		var currentRowWidth = 0f;
		var glyphs = fontGlyphs.ToArray();
		for (var i = 0; i < glyphs.Length; i++)
		{
			var glyph = glyphs[i];
			if (strategy.ShouldLineBreakOn((char)glyph.Code))
			{
				lastValidLineBreakIndex = i;
			}

			var advance = font.ScaleEmToPixelSize(glyph.Advance);
			if (currentRowWidth + advance >= maxWidth && previousRowEndIndex != lastValidLineBreakIndex)
			{
				// Line break
				var endIndex = previousRowEndIndex == lastValidLineBreakIndex ? i : lastValidLineBreakIndex;
				var glyphsInRow = glyphs.Take(new Range(previousRowEndIndex, endIndex));
				var row = new TextRow(font, TrimStart(glyphsInRow, strategy));
				newRows.Add(row);
				previousRowEndIndex = endIndex;
				i = endIndex; // Note that we will immediately increment "i" at the end of the loop.
				currentRowWidth = 0;
			}
			else
			{
				currentRowWidth += advance;
			}
		}

		var finalGlyphs = glyphs.Skip(previousRowEndIndex);
		var finalRow = new TextRow(font, TrimStart(finalGlyphs, strategy));
		newRows.Add(finalRow);
		return newRows;
	}

	private IEnumerable<FontGlyph> TrimStart(IEnumerable<FontGlyph> glyphs, TextBreakStrategy strategy)
	{
		return glyphs.SkipWhile(glyph => strategy.ShouldLineBreakOn((char)glyph.Code));
	}

	public Vector2 CalculateTextSize(string text, float maxWidth = float.PositiveInfinity,
		TextBreakStrategy textBreakStrategy = TextBreakStrategy.Whitespace) =>
		CalculateTextSize(TextToGlyphs(text), maxWidth, textBreakStrategy);

	public Vector2 CalculateTextSize(IEnumerable<FontGlyph> text, float maxWidth = float.PositiveInfinity,
		TextBreakStrategy textBreakStrategy = TextBreakStrategy.Whitespace)
	{
		var rows = LineBreak(text, maxWidth, textBreakStrategy);
		return CalculateTextSize(rows);
	}

	public Vector2 CalculateTextSize(IEnumerable<TextRow> rows, bool skipFirstLine = false)
	{
		var sizes = rows.Select(row => row.Size);

		var width = 0f;
		var height = 0f;

		var isFirst = true;
		foreach (var (x, y) in sizes)
		{
			if (isFirst && skipFirstLine)
			{
				isFirst = false;
				continue;
			}

			width = Math.Max(width, x);
			height += y;
		}

		return new Vector2(width, height);
	}
}