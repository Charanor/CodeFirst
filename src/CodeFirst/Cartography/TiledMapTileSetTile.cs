namespace CodeFirst.Cartography;

public class TiledMapTileSetTile
{
	/// <summary>
	///     The class of the tile. Inherited by tile objects.
	/// </summary>
	/// <remarks>Defaults to <c>string.Empty</c>.</remarks>
	public string Type { get; init; } = string.Empty;

	/// <inheritdoc cref="Type" />
	/// <seealso cref="Type" />
	[Obsolete("Use Type instead")]
	public string Class => Type;

	/// <summary>
	///     A percentage indicating the probability that this tile is chosen when it competes with others while editing
	///     with the terrain tool.
	/// </summary>
	/// <remarks>Not often used programmatically.</remarks>
	public float Probability { get; init; } = 0;

	/// <summary>
	///     The X position of the sub-rectangle representing this tile.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int X { get; init; } = 0;

	/// <summary>
	///     The Y position of the sub-rectangle representing this tile.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Y { get; init; } = 0;

	/// <summary>
	///     The width of the sub-rectangle representing this tile.
	/// </summary>
	/// <remarks>Defaults to the image width.</remarks>
	public required int Width { get; init; }

	/// <summary>
	///     The height of the sub-rectangle representing this tile.
	/// </summary>
	/// <remarks>Defaults to the image height.</remarks>
	public required int Height { get; init; }

	/// <summary>
	///     The image source used for this tile.
	/// </summary>
	public required TileMapTileSetImage Image { get; init; }

	/// <summary>
	///     The animations associated with this tile.
	/// </summary>
	/// <remarks>If this tile is not animated, the collection will be empty.</remarks>
	public required IReadOnlyCollection<TiledMapTileSetTileAnimationFrame> AnimationFrames { get; init; }
	
	/// <summary>
	///		Collision objects (if any) present on this tile.
	/// </summary>
	public required TiledMapObjectGroup? CollisionObjects { get; init; }
}