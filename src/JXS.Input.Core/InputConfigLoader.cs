using System.Xml.Linq;
using JXS.Utils.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public static class InputConfigLoader
{
	private const string ID = "Id";
	private const string POSITIVE = "Positive";
	private const string NEGATIVE = "Negative";
	private const string AXIS = "Axis";
	private const string INVERTED = "Inverted";
	private const string BUTTON = "Button";
	private const string MODIFIER = "Modifier";
	private static readonly ILogger Logger = LoggingManager.Get(nameof(InputConfigLoader));

	public static void LoadConfig(this InputSystem inputSystem, XElement root)
	{
		foreach (var xInput in root.Elements())
		{
			var xId = xInput.Attribute(ID);
			if (xId is null)
			{
				Logger.Warn($"Input config does not have attribute {ID}=\"{ID}\".");
				continue;
			}

			if (!xInput.HasElements)
			{
				Logger.Warn($"Input config {xId.Value} does not have any axis elements.");
				continue;
			}

			foreach (var xAxis in xInput.Elements())
			{
				var xInverted = xAxis.Attribute(INVERTED);
				var inverted = xInverted != null && bool.TryParse(xInverted.Value, out var invVal) && invVal;

				Axis? axis = null;
				switch (xAxis.Name.LocalName)
				{
					case nameof(KeyboardAxis):
					{
						var xPositive = xAxis.Attribute(POSITIVE);
						if (xPositive is null || !Enum.TryParse<Keys>(xPositive.Value, out var positive))
						{
							Logger.Warn(
								$"No attribute {POSITIVE}=\"{POSITIVE}\", OR invalid value ({xPositive?.Value}) on config {xId.Value}.");
							continue;
						}

						var xNegative = xAxis.Attribute(NEGATIVE);
						if (xNegative is null || !Enum.TryParse<Keys>(xNegative.Value, out var negative))
						{
							Logger.Warn(
								$"No attribute {NEGATIVE}=\"{NEGATIVE}\", OR invalid value ({xNegative?.Value}) on config {xId.Value}.");
							continue;
						}

						axis = new KeyboardAxis(positive, negative);
						break;
					}
					case nameof(KeyboardButton):
					{
						var xButtonAttr = xAxis.Attribute(BUTTON);
						if (xButtonAttr is null || !Enum.TryParse<Keys>(xButtonAttr.Value, out var button))
						{
							Logger.Warn(
								$"No attribute {BUTTON}=\"{BUTTON}\", OR invalid value ({xButtonAttr?.Value}) on config {xId.Value}.");
							continue;
						}

						var xModifierAttr = xAxis.Attribute(MODIFIER);
						if (xModifierAttr is null ||
						    !Enum.TryParse<ModifierKey>(xModifierAttr.Value, out var modifier))
						{
							modifier = ModifierKey.None;
						}

						axis = new KeyboardButton(button, modifier);
						break;
					}
					case nameof(MouseButton):
					{
						var xButtonAttr = xAxis.Attribute(BUTTON);
						if (xButtonAttr is null || !Enum.TryParse<Buttons>(xButtonAttr.Value, out var button))
						{
							Logger.Warn(
								$"No attribute {BUTTON}=\"{BUTTON}\", OR invalid value ({xButtonAttr?.Value}) on config {xId.Value}.");
							continue;
						}

						var xModifierAttr = xAxis.Attribute(MODIFIER);
						if (xModifierAttr is null ||
						    !Enum.TryParse<ModifierKey>(xModifierAttr.Value, out var modifier))
						{
							modifier = ModifierKey.None;
						}

						axis = new MouseButton(button, modifier);
						break;
					}
					default:
						Logger.Warn($"Unable to handle axis of type {xAxis.Name.LocalName} on config {xId.Value}.");
						break;
				}

				if (axis != null)
				{
					inputSystem[xId.Value] += inverted ? axis.Negated() : axis;
				}
			}
		}
	}
}