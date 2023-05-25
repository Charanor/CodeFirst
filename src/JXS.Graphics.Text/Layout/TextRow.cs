using OpenTK.Mathematics;

namespace JXS.Graphics.Text.Layout;

public record TextRow(Font Font, IEnumerable<FontGlyph> Glyphs)
{
	public Vector2 Size { get; } = CalculateSize(Font, Glyphs);

	private static Vector2 CalculateSize(Font font, IEnumerable<FontGlyph> glyphs)
	{
		var glyphList = glyphs.ToList();
		var width = glyphList.Sum(glyph => font.ScaleEmToPixelSize(glyph.Advance));
		
		// NOTE: The following logic is, technically, correct. However actually doing this results in weird rendering,
		// so maybe "Advance" actually takes this into account? Idk really, I'm not that great with font rendering.
		//
		// "Advance" includes some extra space, this removes that space. This only needs to be done on the last glyph,
		// since we DO want this space on the other glyphs to separate them properly.
		// var lastGlyph = glyphList[^1];
		// width -= font.ScaleEmToPixelSize(lastGlyph.Advance - font.ScalePixelsToEm(lastGlyph.Size.X));
		var height = glyphList.Max(glyph => glyph.Size.Y);
		return new Vector2(width, height);
	}
}