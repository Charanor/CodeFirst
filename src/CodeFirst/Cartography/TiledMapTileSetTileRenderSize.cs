using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

public enum TiledMapTileSetTileRenderSize
{
	Tile,
	Grid
}

public static class TiledMapTileSetTileRenderSizeExtensions
{
	public static bool TryParseTiledMapTileSetTileRenderSize(this string str,
		[NotNullWhen(true)] out TiledMapTileSetTileRenderSize? renderOrder)
	{
		switch (str)
		{
			case "tile":
				renderOrder = TiledMapTileSetTileRenderSize.Tile;
				return true;
			case "grid":
				renderOrder = TiledMapTileSetTileRenderSize.Grid;
				return true;
		}

		renderOrder = null;
		return false;
	}
}