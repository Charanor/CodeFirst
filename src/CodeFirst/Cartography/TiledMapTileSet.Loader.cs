using System.Xml.Linq;
using CodeFirst.FileSystem;
using JetBrains.Annotations;

namespace CodeFirst.Cartography;

public partial class TiledMapTileSet
{
	public static TiledMapTileSet CreateFromFileHandle(FileHandle handle) => CreateFromXmlString(handle.ReadAllText());

	public static TiledMapTileSet CreateFromXmlString([LanguageInjection(InjectedLanguage.XML)] string xml) =>
		CreateFromXDocument(XDocument.Parse(xml));

	public static TiledMapTileSet CreateFromXDocument(XDocument xDocument)
	{
		// TODO: Implement me
		var root = xDocument.Root;
		if (root is not { Name.LocalName: "tileset" })
		{
			throw new TiledMapParsingException(
				$"Expected root XElement to be named 'tileset', got {root?.Name.LocalName}");
		}

		var xImage = root.Element("image");
		var image = xImage != null
			? new TileMapTileSetImage(
				xImage.StringAttribute("source"),
				xImage.IntAttribute("width"),
				xImage.IntAttribute("height")
			)
			: null;

		var tileCount = root.IntAttribute("tilecount");
		var xTiles = root.Elements("tile");
		var tiles = Enumerable.Range(start: 0, tileCount).Select(id =>
		{
			var xTile = xTiles.FirstOrDefault(xTile => xTile.IntAttribute("id") == id);
			var objectLayer = xTile?.Elements("objectgroup").Select(element =>
			{
				var objectElements = element.Elements("object").Select(xObject =>
				{
					var objectProperties = xObject
						.Element("properties")
						?.Elements("property")
						.ToDictionary(
							xProperty => xProperty.StringAttribute("name"),
							xProperty => new TiledMapProperty(
								TiledMapPropertyTypeExtensions.ParsePropertyType(xProperty.StringAttribute("type")),
								xProperty.StringAttribute("value")
							)
						) ?? new Dictionary<string, TiledMapProperty>();

					return new TiledMapObject
					{
						Id = xObject.IntAttribute("id"),
						Name = xObject.StringAttribute("name", string.Empty),
						Type = xObject.StringAttribute("type", string.Empty),
						Gid = xObject.IntAttribute("gid", defaultValue: 0),
						X = xObject.IntAttribute("x"),
						Y = xObject.IntAttribute("y"),
						Width = xObject.IntAttribute("width", defaultValue: 0),
						Height = xObject.IntAttribute("height", defaultValue: 0),
						Properties = objectProperties
					};
				});
				var properties = new Dictionary<string, TiledMapProperty>();

				return new TiledMapObjectGroup
				{
					Id = element.IntAttribute("id"),
					Name = element.StringAttribute("name", string.Empty),
					Objects = objectElements.ToList().AsReadOnly(),
					Properties = properties.AsReadOnly()
					// TODO: Other attributes :P
				};
			});
			var tileImage = xTile?.Element("image") is { } xTileImage
				? new TileMapTileSetImage(
					xTileImage.StringAttribute("source"),
					xTileImage.IntAttribute("width"),
					xTileImage.IntAttribute("height")
				)
				// Null assertion is valid here since if we don't have an "image" element the parent image must be present
				: image!;

			return new TiledMapTileSetTile
			{
				Type = xTile?.StringAttribute("type", string.Empty) ?? string.Empty,
				X = xTile?.IntAttribute("x", defaultValue: 0) ?? 0,
				Y = xTile?.IntAttribute("y", defaultValue: 0) ?? 0,
				Width = xTile?.IntAttribute("width", tileImage.Width) ?? tileImage.Width,
				Height = xTile?.IntAttribute("height", tileImage.Height) ?? tileImage.Height,
				Image = tileImage,
				// TODO: Implement proper parsing of animation frames
				AnimationFrames = Enumerable.Empty<TiledMapTileSetTileAnimationFrame>().ToList().AsReadOnly(),
				CollisionObjects = objectLayer?.FirstOrDefault()
			};
		});

		return new TiledMapTileSet
		{
			Version = root.StringAttribute("version"),
			TiledVersion = root.StringAttribute("tiledversion", defaultValue: null),
			Name = root.StringAttribute("name"),
			TileWidth = root.IntAttribute("tilewidth"),
			TileHeight = root.IntAttribute("tileheight"),
			TileCount = tileCount,
			Columns = root.IntAttribute("columns"),
			Image = image,
			Tiles = tiles.ToList().AsReadOnly(),
			// TODO: Parse other attributes
		};
	}
}