namespace CodeFirst.Graphics.Core;

public readonly record struct Viewport(int X, int Y, int Width, int Height)
{
	public void Apply() => Viewport(X, Y, Width, Height);
}