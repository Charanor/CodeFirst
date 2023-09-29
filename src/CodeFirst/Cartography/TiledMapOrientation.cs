namespace CodeFirst.Cartography;

public enum TiledMapOrientation
{
	Orthogonal,
	Isometric,
	Staggered
}

public static class TiledMapOrientationExtensions
{
	public static bool TryParseTiledMapOrientation(string str, out TiledMapOrientation orientation)
	{
		switch (str)
		{
			case "orthogonal":
				orientation = TiledMapOrientation.Orthogonal;
				return true;
			case "isometric":
				orientation = TiledMapOrientation.Isometric;
				return true;
			case "staggered":
				orientation = TiledMapOrientation.Staggered;
				return true;
		}

		orientation = TiledMapOrientation.Orthogonal;
		return false;
	}
}