namespace CodeFirst.Cartography;

/// <summary>
///     Represents an image used in a <see cref="TiledMapTileSet" />
/// </summary>
/// <param name="Source">The file path to the source of this image</param>
/// <param name="Width">The width of this image, in pixels</param>
/// <param name="Height">The height of this image, in pixels</param>
public record TileMapTileSetImage(string Source, int Width, int Height);