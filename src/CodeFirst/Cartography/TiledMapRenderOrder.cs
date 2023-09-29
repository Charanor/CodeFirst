using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

public enum TiledMapRenderOrder
{
	RightUp,
	RightDown,
	LeftUp,
	LeftDown
}

public static class TiledMapRenderOrderExtensions
{
	public static bool TryParseTiledMapRenderOrder(string str, out TiledMapRenderOrder renderOrder)
	{
		switch (str)
		{
			case "right-up":
				renderOrder = TiledMapRenderOrder.RightUp;
				return true;
			case "right-down":
				renderOrder = TiledMapRenderOrder.RightDown;
				return true;
			case "left-up":
				renderOrder = TiledMapRenderOrder.LeftUp;
				return true;
			case "left-down":
				renderOrder = TiledMapRenderOrder.LeftDown;
				return true;
		}

		renderOrder = 0;
		return false;
	}
}