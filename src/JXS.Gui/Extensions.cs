using OpenTK.Mathematics;

namespace JXS.Gui;

public static class Extensions
{
	public static Box2i Floor(this Box2 box) => new(
		(int)box.X,
		(int)box.Y,
		(int)box.Width,
		(int)box.Height
	);
}