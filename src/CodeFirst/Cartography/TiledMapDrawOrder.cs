using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

public enum TiledMapDrawOrder
{
	Index,
	TopDown
}

public static class TiledMapDrawOrderExtensions
{
	public static bool TryParseTiledMapDrawOrder(this string str, [NotNullWhen(true)] out TiledMapDrawOrder? drawOrder)
	{
		switch (str)
		{
			case "index":
				drawOrder = TiledMapDrawOrder.Index;
				return true;
			case "topdown":
				drawOrder = TiledMapDrawOrder.TopDown;
				return true;
		}

		drawOrder = null;
		return false;
	}
}