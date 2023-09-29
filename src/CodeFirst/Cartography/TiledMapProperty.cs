namespace CodeFirst.Cartography;

public record TiledMapProperty(TiledMapPropertyType Type, string Value)
{
	/// <summary>
	///     The name of the custom property type, if any. Will only (and always) have a value if <see cref="Type" /> is
	///     <see cref="TiledMapPropertyType.Custom" />.
	/// </summary>
	/// <remarks>Defaults to <c>null</c>, meaning no custom property type.</remarks>
	public string? PropertyType { get; init; }
}

public enum TiledMapPropertyType
{
	String,
	Int,
	Float,
	Bool,
	Color,
	File,
	Object,
	Class,
	Custom
}

public static class TiledMapPropertyTypeExtensions
{
	public static TiledMapPropertyType ParsePropertyType(string str) =>
		str switch
		{
			"string" => TiledMapPropertyType.String,
			"int" => TiledMapPropertyType.Int,
			"float" => TiledMapPropertyType.Float,
			"bool" => TiledMapPropertyType.Bool,
			"color" => TiledMapPropertyType.Color,
			"file" => TiledMapPropertyType.File,
			"class" => TiledMapPropertyType.Class,
			"object" => TiledMapPropertyType.Object,
			_ => 0
		};
}