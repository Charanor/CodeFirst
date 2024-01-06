using System.Globalization;
using CodeFirst.Utils;
using CodeFirst.Utils.Logging;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public static class ColorExtensions
{
	private static readonly ILogger Logger = LoggingManager.Get(nameof(ColorExtensions));

	public static Color4<Rgba> ToColor(this string cssColor)
	{
		try
		{
			if (cssColor.StartsWith("#"))
			{
				switch (cssColor.Length)
				{
					case 4:
					{
						// Format #rgb
						var r = int.Parse($"{cssColor[1]}{cssColor[1]}", NumberStyles.HexNumber) / 255f;
						var g = int.Parse($"{cssColor[2]}{cssColor[2]}", NumberStyles.HexNumber) / 255f;
						var b = int.Parse($"{cssColor[3]}{cssColor[3]}", NumberStyles.HexNumber) / 255f;
						return new Color4<Rgba>(r, g, b, w: 1);
					}
					case 5:
					{
						// Format #rgba
						var r = int.Parse(cssColor[1].ToString(), NumberStyles.HexNumber) / 255f;
						var g = int.Parse(cssColor[2].ToString(), NumberStyles.HexNumber) / 255f;
						var b = int.Parse(cssColor[3].ToString(), NumberStyles.HexNumber) / 255f;
						var a = int.Parse(cssColor[4].ToString(), NumberStyles.HexNumber) / 255f;
						return new Color4<Rgba>(r, g, b, a);
					}
					case 7:
					{
						// Format #rrggbb
						var r = int.Parse(cssColor[1..3], NumberStyles.HexNumber) / 255f;
						var g = int.Parse(cssColor[3..5], NumberStyles.HexNumber) / 255f;
						var b = int.Parse(cssColor[5..7], NumberStyles.HexNumber) / 255f;
						return new Color4<Rgba>(r, g, b, w: 1);
					}
					case 9:
					{
						// Format #rrggbbaa
						var r = int.Parse(cssColor[1..3], NumberStyles.HexNumber) / 255f;
						var g = int.Parse(cssColor[3..5], NumberStyles.HexNumber) / 255f;
						var b = int.Parse(cssColor[5..7], NumberStyles.HexNumber) / 255f;
						var a = int.Parse(cssColor[7..9], NumberStyles.HexNumber) / 255f;
						return new Color4<Rgba>(r, g, b, a);
					}
				}
			}
			else if (cssColor.StartsWith("rgba"))
			{
				// Format rgba(r,g,b,a)
				var values = cssColor.Split("(")[1].Split(")")[0].Split(",").Select(float.Parse).ToList();
				return new Color4<Rgba>(values[0] / 255f, values[1] / 255f, values[2] / 255f, values[3] / 255f);
			}
			else if (cssColor.StartsWith("rgb"))
			{
				// Format rgb(r,g,b)
				var values = cssColor.Split("(")[1].Split(")")[0].Split(",").Select(float.Parse).ToList();
				return new Color4<Rgba>(values[0], values[1], values[2], w: 1);
			}
		}
		catch (Exception e)
		{
			DevTools.ThrowStatic(typeof(ColorExtensions), e);
			Logger.Error($"Failed to parse color {cssColor} {e}");
			return default;
		}

		Logger.Warn($"Failed to parse color {cssColor}");
		return default;
	}
}