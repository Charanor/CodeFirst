using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public record BorderSize
{
	public static readonly BorderSize Zero = new(top: 0, right: 0, bottom: 0, left: 0);

	public BorderSize()
	{
	}

	public BorderSize(float top, float right, float bottom, float left)
	{
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}

	public float Top { get; init; }
	public float Right { get; init; }
	public float Left { get; init; }
	public float Bottom { get; init; }

	public void Deconstruct(out float top, out float right, out float bottom, out float left)
	{
		top = Top;
		right = Right;
		bottom = Bottom;
		left = Left;
	}

	public static BorderSize operator +(BorderSize left, BorderSize right) => new(
		left.Top + right.Top,
		left.Right + right.Right,
		left.Bottom + right.Bottom,
		left.Left + right.Left
	);

	public static implicit operator BorderSize(Box2 box) => new(
		box.Top,
		box.Right,
		box.Bottom,
		box.Left
	);

	public static implicit operator BorderSize(Box2i box) => new(
		box.Top,
		box.Right,
		box.Bottom,
		box.Left
	);
}

public static class NinePatchExtensions
{
	public static BorderSize ToBorderSize(this NinePatch ninePatch) => ninePatch.ContentPadding;
}