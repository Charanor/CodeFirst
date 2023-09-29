using CodeFirst.Utils;

namespace CodeFirst.Cartography;

public class TiledMapLayer : TiledMapLayerBase
{
	/// <summary>
	///     The width of the layer, in tiles.
	/// </summary>
	public required int Width { get; init; }

	/// <summary>
	///     The height of the layer, in tiles.
	/// </summary>
	public required int Height { get; init; }
	
	public int[,] Tiles { get; init; }
	
	public int this[int column, int row]
	{
		get
		{
			if (column < 0 || column >= Width)
			{
				DevTools.Throw<TiledMapLayer>(new IndexOutOfRangeException($"0 <= column({column}) < {Width}"));
				return 0;
			}

			if (row < 0 || row >= Height)
			{
				DevTools.Throw<TiledMapLayer>(new IndexOutOfRangeException($"0 <= row({row}) < {Height}"));
				return 0;
			}

			return Tiles[column, row];
		}
	}

	// TODO: Add support for offset
	// TODO: Add support for parallax
}