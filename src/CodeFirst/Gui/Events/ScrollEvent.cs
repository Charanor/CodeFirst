using OpenTK.Mathematics;

namespace CodeFirst.Gui.Events;

public class ScrollEvent : UiEvent
{
	/// <summary>
	///     How much was scrolled.
	/// </summary>
	public Vector2 Delta { get; init; }

	/// <summary>
	///     How much the content is offset from its initial position. This is a value between <c>(0, 0)</c> and
	///		<c>(maxX, maxY)</c> where <c>maxX = parentWidth - selfWidth</c> and <c>maxY = parentHeight - selfHeight</c>.
	///		A value of <c>(0, 0)</c> indicates vertical scroll is at the top and horizontal scroll is on the left.
	/// </summary>
	/// <seealso cref="NormalizedOffset"/>
	public Vector2 Offset { get; init; }
	
	/// <summary>
	///		How much the content is offset from its initial position. This is a value between <c>(0, 0)</c> and
	///		<c>(1, 1)</c> where a value of <c>(0, 0)</c> indicates vertical scroll is at the top and horizontal scroll
	///		is on the left and a value of <c>(1, 1)</c> indicates vertical scroll is at the bottom and horizontal
	///		scroll is on the right.
	/// </summary>
	/// <seealso cref="Offset"/>
	public Vector2 NormalizedOffset { get; init; }
}