using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Layout;

public record TextRow(Font Font, IEnumerable<FontGlyph> Glyphs)
{
	public Vector2 Size { get; } = CalculateSize(Font, Glyphs);

	private static Vector2 CalculateSize(Font font, IEnumerable<FontGlyph> glyphs)
	{
		var width = glyphs.Sum(glyph => font.ScaleEmToPixelSize(glyph.Advance));
		return new Vector2(width, font.ScaleEmToPixelSize(font.Metrics.LineHeight));
	}
}