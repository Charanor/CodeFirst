using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Cartography;

[Flags]
public enum TiledMapTileSetObjectAlignment
{
	Unspecified = 0,
	Top = 1 << 0,
	Bottom = 1 << 1,
	VerticalCenter = 1 << 2,
	Left = 1 << 3,
	Right = 1 << 4,
	HorizontalCenter = 1 << 5,
	TopLeft = Top | Left,
	TopRight = Top | Right,
	TopCenter = Top | HorizontalCenter,
	BottomLeft = Bottom | Left,
	BottomRight = Bottom | Right,
	BottomCenter = Bottom | HorizontalCenter,
	CenterLeft = VerticalCenter | Left,
	Middle = VerticalCenter | HorizontalCenter,
	CenterRight = VerticalCenter | Right,
}

public static class TiledMapTileSetObjectAlignmentExtensions
{
	public static bool TryParseTiledMapTileSetObjectAlignment(this string str,
		[NotNullWhen(true)] out TiledMapTileSetObjectAlignment? renderOrder)
	{
		switch (str)
		{
			case "unspecified":
				renderOrder = TiledMapTileSetObjectAlignment.Unspecified;
				return true;
			case "topleft":
				renderOrder = TiledMapTileSetObjectAlignment.TopLeft;
				return true;
			case "top":
				renderOrder = TiledMapTileSetObjectAlignment.TopCenter;
				return true;
			case "topright":
				renderOrder = TiledMapTileSetObjectAlignment.TopRight;
				return true;
			case "left":
				renderOrder = TiledMapTileSetObjectAlignment.CenterLeft;
				return true;
			case "center":
				renderOrder = TiledMapTileSetObjectAlignment.Middle;
				return true;
			case "right":
				renderOrder = TiledMapTileSetObjectAlignment.CenterRight;
				return true;
			case "bottomleft":
				renderOrder = TiledMapTileSetObjectAlignment.BottomLeft;
				return true;
			case "bottom":
				renderOrder = TiledMapTileSetObjectAlignment.BottomCenter;
				return true;
			case "bottomright":
				renderOrder = TiledMapTileSetObjectAlignment.BottomRight;
				return true;
		}

		renderOrder = null;
		return false;
	}
}