using OpenTK.Mathematics;

namespace CodeFirst.Cartography;

public partial class TiledMapTileSet
{
	/// <summary>
	///     The name of this tile set.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	///     The TMX format version.
	/// </summary>
	public required string Version { get; init; }

	/// <summary>
	///     The Tiled version used to save the file. May be a date for snapshot builds.
	/// </summary>
	/// <remarks>Can be <c>null</c> if version was unknown.</remarks>
	public string? TiledVersion { get; init; } = null;

	/// <summary>
	///     The class of this tile set, as set in the Tiled editor.
	/// </summary>
	/// <remarks>Defaults to <c>string.Empty</c>.</remarks>
	public string Class { get; init; }

	/// <summary>
	///     The maximum width of a tile, in pixels.
	/// </summary>
	public required int TileWidth { get; init; }

	/// <summary>
	///     The maximum height of a tile, in pixels.
	/// </summary>
	public required int TileHeight { get; init; }

	/// <summary>
	///     The maximum size of a tile, in pixels.
	/// </summary>
	public Vector2i TileSize => (TileWidth, TileHeight);

	/// <summary>
	///     The spacing (in pixels) between the tiles in this tile set.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Spacing { get; init; }

	/// <summary>
	///     The margin around the tiles in this tile set.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Margin { get; init; }

	/// <summary>
	///     The number of tiles in this tile set.
	/// </summary>
	/// <remarks>
	///     Note that there can be tiles with a higher ID than the tile count, in case the tile set
	///     is an image collection from which tiles have been removed.
	/// </remarks>
	public required int TileCount { get; init; }

	/// <summary>
	///     The number of columns in the tile set.
	/// </summary>
	public required int Columns { get; init; }

	/// <summary>
	///     Controls the alignment for tile objects.
	/// </summary>
	/// <remarks>
	///     When <see cref="TiledMapTileSetObjectAlignment.Unspecified" /> tile objects use
	///     <see cref="TiledMapTileSetObjectAlignment.BottomLeft" /> in orthogonal mode and
	///     <see cref="TiledMapTileSetObjectAlignment.BottomCenter" /> in isometric mode.
	/// </remarks>
	public TiledMapTileSetObjectAlignment ObjectAlignment { get; init; } = TiledMapTileSetObjectAlignment.Unspecified;

	/// <summary>
	///     The size to use when rendering tiles from this tile set on a tile layer.
	/// </summary>
	/// <remarks>
	///     When set to <see cref="TiledMapTileSetTileRenderSize.Grid" />, the tile is drawn at the tile grid
	///     size of the map.
	/// </remarks>
	public TiledMapTileSetTileRenderSize TileRenderSize { get; init; } = TiledMapTileSetTileRenderSize.Grid;

	/// <summary>
	///     The fill mode to use when rendering tiles from this tile set.
	/// </summary>
	/// <remarks>
	///     Only relevant when the tiles are not rendered at their native size, so this applies to resized tile objects
	///     or in combination with <see cref="TileRenderSize" /> set to <see cref="TiledMapTileSetTileRenderSize.Grid" />.
	/// </remarks>
	public TiledMapTileSetFillMode FillMode { get; init; } = TiledMapTileSetFillMode.Stretch;

	/// <summary>
	///     Represents the tile set image, if this tile set was made from a singular image.
	/// </summary>
	/// <remarks>Will be <c>null</c> if the images in this tile set are individual source images.</remarks>
	public TileMapTileSetImage? Image { get; init; } = null;
	
	/// <summary>
	///		The tiles contained in this tile set, indexed by their local id.
	/// </summary>
	public required IReadOnlyList<TiledMapTileSetTile> Tiles { get; init; }
}