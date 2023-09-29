using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

public enum TiledMapStaggerAxis
{
	X,
	Y
}

public static class TiledMapStaggerAxisExtensions
{
	public static bool TryParseTiledMapStaggerAxis(this string str,
		[NotNullWhen(true)] out TiledMapStaggerAxis? renderOrder)
	{
		switch (str)
		{
			case "x":
				renderOrder = TiledMapStaggerAxis.X;
				return true;
			case "y":
				renderOrder = TiledMapStaggerAxis.Y;
				return true;
		}

		renderOrder = null;
		return false;
	}
}