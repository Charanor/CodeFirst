namespace CodeFirst.Cartography;

/// <summary>
///     Represents a single frame of an animated tile.
/// </summary>
/// <param name="TileId">The local id of a tile within the parent <see cref="TiledMapTileSet" /></param>
/// <param name="Duration">How long (in milliseconds) this frame should be displayed before advancing to the next frame.</param>
public record TiledMapTileSetTileAnimationFrame(int TileId, float Duration);