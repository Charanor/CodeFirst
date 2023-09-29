using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

public enum TiledMapTileSetFillMode
{
	Stretch,
	PreserveAspectFit
}

public static class TiledMapTileSetFillModeExtensions
{
	public static bool TryParseTiledMapTileSetFillMode(this string str,
		[NotNullWhen(true)] out TiledMapTileSetFillMode? renderOrder)
	{
		switch (str)
		{
			case "stretch":
				renderOrder = TiledMapTileSetFillMode.Stretch;
				return true;
			case "preserve-aspect-fit":
				renderOrder = TiledMapTileSetFillMode.PreserveAspectFit;
				return true;
		}

		renderOrder = null;
		return false;
	}
}