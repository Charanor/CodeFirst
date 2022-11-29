using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Layout;

public record TextRow(Font Font, IEnumerable<FontGlyph> Glyphs)
{
	public Vector2 Size { get; } = CalculateSize(Font, Glyphs);

	private static Vector2 CalculateSize(Font font, IEnumerable<FontGlyph> glyphs)
	{
		var width = 0f;
		var height = 0f;

		FontGlyph? previousGlyph = null;
		foreach (var glyph in glyphs)
		{
			var kerning = previousGlyph == null ? 0 : font.GetKerningBetween(previousGlyph, glyph);
			width += glyph.Size.X + glyph.Advance + kerning;
			height = Math.Max(height, glyph.Size.Y);
			previousGlyph = glyph;
		}

		return new Vector2(width, height);
	}
}