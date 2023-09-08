using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Text;

/// <summary>
///     Represents a glyph inside of a font.
/// </summary>
/// <param name="Code">the code (often unicode) of this glyph's character</param>
/// <param name="Position">the position of the glyph, in pixels, inside the parent font's texture atlas</param>
/// <param name="Size">the size of the glyph, in pixels, inside the parent font's texture atlas</param>
/// <param name="Advance">how much to advance the virtual cursor after rendering this glyph, in EM:s</param>
/// <param name="Offset">
///     when rendering this glyph to a quad, this is the offset on how to place this glyph relative to the
///     "expected" drawing position
/// </param>
public record FontGlyph(int Code, Vector2 Position, Vector2 Size, float Advance, Box2 Offset)
{
	/// <summary>
	///     This glyph's bounds on the parent texture atlas in pixels.
	/// </summary>
	/// <remarks>Might be non-integer values</remarks>
	public Box2 AtlasBounds => Box2.FromSize(Position, Size);
}