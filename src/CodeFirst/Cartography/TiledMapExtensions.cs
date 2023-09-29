using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using CodeFirst.Gui;

namespace CodeFirst.Cartography;

internal static class TiledMapExtensions
{
	public delegate bool EnumParser<TEnum>(string str, [NotNullWhen(true)] out TEnum? result);

	public static string StringAttribute(this XElement element, string attributeName)
	{
		if (!element.StringAttribute(attributeName, out var value))
		{
			throw new TiledMapParsingException(
				$"Required attribute '{attributeName}' of element '{element.Name.LocalName}' was not present");
		}

		return value;
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static string? StringAttribute(this XElement element, string attributeName, string? defaultValue) =>
		element.StringAttribute(attributeName, out var value) ? value : defaultValue;

	public static int IntAttribute(this XElement element, string attributeName)
	{
		if (!element.IntAttribute(attributeName, out var value))
		{
			throw new TiledMapParsingException(
				$"Required attribute '{attributeName}' of element '{element.Name.LocalName}' was not present");
		}

		return value;
	}

	public static int IntAttribute(this XElement element, string attributeName, int defaultValue) =>
		element.IntAttribute(attributeName, out var value) ? value : defaultValue;

	public static TEnum EnumAttribute<TEnum>(this XElement element, string attributeName, EnumParser<TEnum> parser)
	{
		if (!element.StringAttribute(attributeName, out var value) || !parser(value, out var enumValue))
		{
			throw new TiledMapParsingException(
				$"Required attribute '{attributeName}' of element '{element.Name.LocalName}' was not present");
		}

		return enumValue;
	}
}