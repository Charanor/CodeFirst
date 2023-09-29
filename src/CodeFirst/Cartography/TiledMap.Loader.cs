using System.Xml.Linq;
using CodeFirst.FileSystem;
using CodeFirst.Gui;
using JetBrains.Annotations;

namespace CodeFirst.Cartography;

public partial class TiledMap
{
	public static TiledMap CreateFromFileHandle(FileHandle handle) =>
		CreateFromXDocument(XDocument.Parse(handle.ReadAllText()), handle);

	public static TiledMap CreateFromXmlString([LanguageInjection(InjectedLanguage.XML)] string xml,
		FileHandle baseFilePath) =>
		CreateFromXDocument(XDocument.Parse(xml), baseFilePath);

	public static TiledMap CreateFromXDocument(XDocument xDocument, FileHandle baseFilePath)
	{
		var root = xDocument.Root;
		if (root is not { Name.LocalName: "map" })
		{
			throw new TiledMapParsingException($"Expected root XElement to be named 'map', got {root?.Name.LocalName}");
		}

		var tileSets = root.Elements("tileset").Select(element =>
		{
			var fileHandle = baseFilePath.Sibling(element.StringAttribute("source"));
			var firstGid = element.IntAttribute("firstgid");
			return (firstGid, TiledMapTileSet.CreateFromFileHandle(fileHandle));
		});

		var tileLayers = root.Elements("layer").Select(element =>
		{
			var width = element.IntAttribute("width");
			var height = element.IntAttribute("height");
			var tiles = new int[width, height];

			var values = element.Element("data")?.Value.Split(",");
			if (values == null)
			{
				throw new TiledMapParsingException(
					$"Required element 'data' of element '{element.Name.LocalName}' not found");
			}

			for (var column = 0; column < width; column++)
			for (var row = 0; row < height; row++)
			{
				tiles[column, row] = int.Parse(values[row * width + column]);
			}

			var properties = new Dictionary<string, TiledMapProperty>();

			return new TiledMapLayer
			{
				Id = element.IntAttribute("id"),
				Name = element.StringAttribute("name"),
				Width = width,
				Height = height,
				Tiles = tiles,
				Properties = properties.AsReadOnly()
				// TODO: Other attributes :P
			};
		});

		var objectLayers = root.Elements("objectgroup").Select(element =>
		{
			var objectElements = element.Elements("object").Select(xObject =>
			{
				var objectProperties = xObject
					.Element("properties")
					?.Elements("property")
					.ToDictionary(
						xProperty => xProperty.StringAttribute("name"),
						xProperty =>
						{
							if (xProperty.StringAttribute("type", out var typeStr))
							{
								return new TiledMapProperty(
									TiledMapPropertyTypeExtensions.ParsePropertyType(typeStr),
									xProperty.StringAttribute("value")
								);
							}

							return new TiledMapProperty(
								TiledMapPropertyType.Custom,
								xProperty.StringAttribute("value")
							)
							{
								PropertyType = xProperty.StringAttribute("propertytype")
							};
						}
					) ?? new Dictionary<string, TiledMapProperty>();

				return new TiledMapObject
				{
					Id = xObject.IntAttribute("id"),
					Name = xObject.StringAttribute("name"),
					Type = xObject.StringAttribute("type"),
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
				Name = element.StringAttribute("name"),
				Objects = objectElements.ToList().AsReadOnly(),
				Properties = properties.AsReadOnly()
				// TODO: Other attributes :P
			};
		});

		return new TiledMap
		{
			Version = root.StringAttribute("version"),
			TiledVersion = root.StringAttribute("tiledversion", defaultValue: null),
			Width = root.IntAttribute("width"),
			Height = root.IntAttribute("height"),
			TileWidth = root.IntAttribute("tilewidth"),
			TileHeight = root.IntAttribute("tileheight"),
			IsInfinite = root.IntAttribute("infinite", defaultValue: 0) != 0,
			Orientation = root.EnumAttribute<TiledMapOrientation>("orientation",
				TiledMapOrientationExtensions.TryParseTiledMapOrientation),
			RenderOrder = root.EnumAttribute<TiledMapRenderOrder>("renderorder",
				TiledMapRenderOrderExtensions.TryParseTiledMapRenderOrder),
			NextLayerId = root.IntAttribute("nextlayerid"),
			NextObjectId = root.IntAttribute("nextobjectid"),
			TileSets = tileSets,
			Layers = tileLayers
				.OfType<TiledMapLayerBase>()
				.Concat(objectLayers)
				.OrderBy(layer => layer.Id)
				.ToList()
				.AsReadOnly()
			// TODO: Initialize all the other stuff :P
		};
	}
}

public class TiledMapParsingException : Exception
{
	public TiledMapParsingException(string? message) : base(message)
	{
	}
}