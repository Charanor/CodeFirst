using OpenTK.Mathematics;

namespace CodeFirst.Cartography;

public partial class TiledMap
{
	/// <summary>
	///     The TMX format version.
	/// </summary>
	public required string Version { get; init; }

	/// <summary>
	///     The Tiled version used to save the file. May be a date for snapshot builds.
	/// </summary>
	/// <remarks>Can be <c>null</c> if version was unknown.</remarks>
	public required string? TiledVersion { get; init; }

	/// <summary>
	///     The class of the map as set in the "class" field in the Tiled editor.
	/// </summary>
	/// <remarks>If no class was set this defaults to <c>string.Empty</c>.</remarks>
	public string Class { get; init; } = string.Empty;

	/// <summary>
	///     The orientation of the map as set in the Tiled editor.
	/// </summary>
	public required TiledMapOrientation Orientation { get; init; }

	/// <summary>
	///     The render order of the map as set in the Tiled editor.
	/// </summary>
	public required TiledMapRenderOrder RenderOrder { get; init; }

	/// <summary>
	///     The compression level used for tile layer data.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>-1</c>, which means to use the algorithm default.
	/// </remarks>
	public int CompressionLevel { get; init; } = -1;

	/// <summary>
	///     The width of the map, in tiles.
	/// </summary>
	public required int Width { get; init; }

	/// <summary>
	///     The height of the map, in tiles.
	/// </summary>
	public required int Height { get; init; }

	/// <summary>
	///     The size of the map, in tiles.
	/// </summary>
	public Vector2i Size => (Width, Height);

	/// <summary>
	///     The width of a tile, in pixels.
	/// </summary>
	public required int TileWidth { get; init; }

	/// <summary>
	///     The height of a tile, in pixels.
	/// </summary>
	public required int TileHeight { get; init; }

	/// <summary>
	///     The size of a tile, in pixels.
	/// </summary>
	public Vector2i TileSize => (TileWidth, TileHeight);

	/// <summary>
	///     Only for hexagonal maps. Determines the width or height (depending on the staggered axis) of the tile's edge,
	///     in pixels.
	/// </summary>
	/// <remarks>For non-hexagonal maps this defaults to <c>0</c>.</remarks>
	public int HexSideLength { get; init; } = 0;

	/// <summary>
	///     For staggered and hexagonal maps, determines which axis (x or y) is staggered.
	/// </summary>
	/// <remarks>For non-staggered and non-hexagonal maps this defaults to <c>null</c>.</remarks>
	public TiledMapStaggerAxis? StaggerAxis { get; init; } = null;

	/// <summary>
	///     For staggered and hexagonal maps, determines whether the "even" or "odd" indexes along the
	///     staggered axis are shifted.
	/// </summary>
	/// <remarks>For non-hexagonal and non-staggered maps this defaults to <c>0</c>.</remarks>
	public int StaggerIndex { get; init; } = 0;

	/// <summary>
	///     X coordinate of the parallax origin in pixel.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int ParallaxOriginX { get; init; } = 0;

	/// <summary>
	///     Y coordinate of the parallax origin in pixel.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int ParallaxOriginY { get; init; } = 0;

	/// <summary>
	///     X and Y coordinates of the parallax origin in pixel.
	/// </summary>
	/// <remarks>Defaults to <c>(0, 0)</c>.</remarks>
	public Vector2i ParallaxOrigin => (ParallaxOriginX, ParallaxOriginY);

	/// <summary>
	///     The background color of the map.
	/// </summary>
	/// <remarks>Defaults to transparent.</remarks>
	public Color4<Rgba> BackgroundColor { get; init; } = new(x: 0, y: 0, z: 0, w: 0);

	/// <summary>
	///     Stores the next available ID for new layers. This number is stored to prevent reuse of the same ID after
	///     layers have been removed.
	/// </summary>
	public required int NextLayerId { get; init; }

	/// <summary>
	///     Stores the next available ID for new objects. This number is stored to prevent reuse of the same ID after
	///     objects have been removed.
	/// </summary>
	public required int NextObjectId { get; init; }

	/// <summary>
	///     Whether this map is infinite. An infinite map has no fixed size and can grow in all directions.
	///     Its layer data is stored in chunks.
	/// </summary>
	public required bool IsInfinite { get; init; }

	/// <summary>
	///     The tile sets coupled with the first GID that that tile set refers to.
	/// </summary>
	public required IEnumerable<(int FirstGid, TiledMapTileSet TileSet)> TileSets { get; init; }

	/// <summary>
	///     The layers
	/// </summary>
	public required IReadOnlyList<TiledMapLayerBase> Layers { get; init; }

	public (int FirstGid, TiledMapTileSet TileSet) FindTileSetForTileGid(int tileGid)
	{
		(int FirstGid, TiledMapTileSet TileSet)? previousTileSetInfo = null;
		foreach (var tileSetInfo in TileSets)
		{
			if (tileSetInfo.FirstGid > tileGid)
			{
				break;
			}

			previousTileSetInfo = tileSetInfo;
		}

		return previousTileSetInfo ??
		       throw new IndexOutOfRangeException($"No tile set contains tile index {tileGid}");
	}

	public TiledMapTileSetTile FindTileForGid(int tileGid)
	{
		var info = FindTileSetForTileGid(tileGid);
		var localId = tileGid - info.FirstGid;
		return info.TileSet.Tiles[localId];
	}
}