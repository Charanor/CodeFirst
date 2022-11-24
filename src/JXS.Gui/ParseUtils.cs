using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenTK.Mathematics;

namespace JXS.Gui;

public static class ParseUtils
{
	public static bool ParseInvariantFloat(this string @this, out float val) =>
		float.TryParse(@this, NumberStyles.Float, CultureInfo.InvariantCulture, out val);

	public static bool FloatAttribute(this XElement @this, string attr, out float val)
	{
		var xAttr = @this.Attribute(attr);
		if (xAttr != null)
		{
			return xAttr.Value.ParseInvariantFloat(out val);
		}

		val = 0;
		return false;
	}

	public static bool IntAttribute(this XElement @this, string attr, out int val)
	{
		var xAttr = @this.Attribute(attr);
		if (xAttr != null)
		{
			return int.TryParse(xAttr.Value, out val);
		}

		val = 0;
		return false;
	}

	public static bool EnumAttribute<T>(this XElement @this, string attr, out T val) where T : struct
	{
		var xAttr = @this.Attribute(attr);
		if (xAttr != null)
		{
			return Enum.TryParse(xAttr.Value, ignoreCase: true, out val);
		}

		val = default;
		return false;
	}

	public static bool StringAttribute(this XElement @this, string attr, out string val)
	{
		var xAttr = @this.Attribute(attr);
		if (xAttr != null)
		{
			val = xAttr.Value;
			return true;
		}

		val = "";
		return false;
	}

	public static XElement? ElementIgnoreNamespace(this XElement @this, XName name) =>
		@this.Elements().FirstOrDefault(e => e.Name.LocalName == name.LocalName);

	public static int ParseInt(string? intString, int defaultValue, NumberStyles style = NumberStyles.Integer) =>
		int.TryParse(intString, style, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;

	public static bool ParseBool(string? boolString, bool defaultValue) =>
		bool.TryParse(boolString, out var value) ? value : defaultValue;

	public static Color4<Rgba> ParseColor(string? colorString, Color4<Rgba> defaultValue)
	{
		if (colorString is null)
		{
			return defaultValue;
		}

		if (colorString.StartsWith("rgba"))
		{
			var match = Regex.Match(colorString, pattern: @"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)");
			if (!match.Success)
			{
				return defaultValue;
			}

			var r = ParseInt(match.Groups[1].Value, defaultValue: 0);
			var g = ParseInt(match.Groups[2].Value, defaultValue: 0);
			var b = ParseInt(match.Groups[3].Value, defaultValue: 0);
			var a = ParseInt(match.Groups[4].Value, defaultValue: 0);
			return new Color4<Rgba>(r, g, b, a);
		}

		if (colorString.StartsWith("#"))
		{
			var match = Regex.Match(colorString,
				pattern: @"^#([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})?$");
			if (!match.Success)
			{
				return defaultValue;
			}

			var r = ParseInt(match.Groups[1].Value, defaultValue: 0, NumberStyles.HexNumber);
			var g = ParseInt(match.Groups[2].Value, defaultValue: 0, NumberStyles.HexNumber);
			var b = ParseInt(match.Groups[3].Value, defaultValue: 0, NumberStyles.HexNumber);
			var a = match.Groups.Count >= 5
				? ParseInt(match.Groups[4].Value, defaultValue: 0, NumberStyles.HexNumber)
				: byte.MaxValue;
			return new Color4<Rgba>(r, g, b, a);
		}

		foreach (var colorProperty in typeof(Color4).GetProperties(BindingFlags.Static | BindingFlags.Public))
		{
			if (colorProperty.Name != colorString)
			{
				continue;
			}

			return colorProperty.GetValue(null) is Color4<Rgba> color ? color : defaultValue;
		}

		return defaultValue;
	}
}