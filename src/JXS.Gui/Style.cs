using System.Xml.Linq;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace JXS.Gui;

public record Style
{
	public static readonly string ValuePrefix = "$";
	public static readonly string ValueEscapeSequence = "$$";

	// Used in "Combine" to check what the default values are :)
	private static readonly Style Default = new();
	private static Random? r;

	public Style()
	{
	}

	public Style(XElement element, IReadOnlyDictionary<string, string> values)
	{
		Position = ParseEnum(GetValue(element, nameof(Position), values), Position);
		Display = ParseEnum(GetValue(element, nameof(Display), values), Display);
		Overflow = ParseEnum(GetValue(element, nameof(Overflow), values), Overflow);
		Flex = ParseFloat(GetValue(element, nameof(Flex), values), Flex);
		FlexDirection = ParseEnum(GetValue(element, nameof(FlexDirection), values), FlexDirection);
		JustifyContent = ParseEnum(GetValue(element, nameof(JustifyContent), values), JustifyContent);
		AlignContent = ParseEnum(GetValue(element, nameof(AlignContent), values), AlignContent);
		AlignItems = ParseEnum(GetValue(element, nameof(AlignItems), values), AlignItems);
		AlignSelf = ParseEnum(GetValue(element, nameof(AlignSelf), values), AlignSelf);
		Margin = ParseYogaValue(GetValue(element, nameof(Margin), values), Margin);
		MarginLeft = ParseYogaValue(GetValue(element, nameof(MarginLeft), values), MarginLeft);
		MarginRight = ParseYogaValue(GetValue(element, nameof(MarginRight), values), MarginRight);
		MarginTop = ParseYogaValue(GetValue(element, nameof(MarginTop), values), MarginTop);
		MarginBottom = ParseYogaValue(GetValue(element, nameof(MarginBottom), values), MarginBottom);
		MarginVertical = ParseYogaValue(GetValue(element, nameof(MarginVertical), values), MarginVertical);
		MarginHorizontal = ParseYogaValue(GetValue(element, nameof(MarginHorizontal), values), MarginHorizontal);
		Padding = ParseYogaValue(GetValue(element, nameof(Padding), values), Padding);
		PaddingLeft = ParseYogaValue(GetValue(element, nameof(PaddingLeft), values), PaddingLeft);
		PaddingRight = ParseYogaValue(GetValue(element, nameof(PaddingRight), values), PaddingRight);
		PaddingTop = ParseYogaValue(GetValue(element, nameof(PaddingTop), values), PaddingTop);
		PaddingBottom = ParseYogaValue(GetValue(element, nameof(PaddingBottom), values), PaddingBottom);
		PaddingVertical = ParseYogaValue(GetValue(element, nameof(PaddingVertical), values), PaddingVertical);
		PaddingHorizontal = ParseYogaValue(GetValue(element, nameof(PaddingHorizontal), values), PaddingHorizontal);
		Width = ParseYogaValue(GetValue(element, nameof(Width), values), Width);
		Height = ParseYogaValue(GetValue(element, nameof(Height), values), Height);
		Left = ParseYogaValue(GetValue(element, nameof(Left), values), Left);
		Right = ParseYogaValue(GetValue(element, nameof(Right), values), Right);
		Top = ParseYogaValue(GetValue(element, nameof(Top), values), Top);
		Bottom = ParseYogaValue(GetValue(element, nameof(Bottom), values), Bottom);
		MinWidth = ParseYogaValue(GetValue(element, nameof(MinWidth), values), MinWidth);
		MinHeight = ParseYogaValue(GetValue(element, nameof(MinHeight), values), MinHeight);
		MaxWidth = ParseYogaValue(GetValue(element, nameof(MaxWidth), values), MaxWidth);
		MaxHeight = ParseYogaValue(GetValue(element, nameof(MaxHeight), values), MaxHeight);
		BackgroundColor =
			ParseUtils.ParseColor(GetValue(element, nameof(BackgroundColor), values), BackgroundColor);
		BorderWidth = ParseFloat(GetValue(element, nameof(BorderWidth), values), BorderWidth);
		BorderLeftWidth = ParseFloat(GetValue(element, nameof(BorderLeftWidth), values), BorderLeftWidth);
		BorderRightWidth = ParseFloat(GetValue(element, nameof(BorderRightWidth), values), BorderRightWidth);
		BorderBottomWidth = ParseFloat(GetValue(element, nameof(BorderBottomWidth), values), BorderBottomWidth);
		BorderTopWidth = ParseFloat(GetValue(element, nameof(BorderTopWidth), values), BorderTopWidth);
		BorderRadius = ParseFloat(GetValue(element, nameof(BorderRadius), values), BorderRadius);
		BorderLeftRadius = ParseFloat(GetValue(element, nameof(BorderLeftRadius), values), BorderLeftRadius);
		BorderRightRadius = ParseFloat(GetValue(element, nameof(BorderRightRadius), values), BorderRightRadius);
		BorderBottomRadius = ParseFloat(GetValue(element, nameof(BorderBottomRadius), values), BorderBottomRadius);
		BorderTopRadius = ParseFloat(GetValue(element, nameof(BorderTopRadius), values), BorderTopRadius);
	}

	private static Color4<Rgba> RandomColor
	{
		get
		{
			r ??= new Random();
			return new Color4<Rgba>((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), w: 1);
		}
	}

	public YogaPositionType Position { get; init; } = YogaPositionType.Relative;
	public YogaDisplay Display { get; init; } = YogaDisplay.Flex;
	public YogaOverflow Overflow { get; init; } = YogaOverflow.Visible;

	public float Flex { get; init; }
	public YogaFlexDirection FlexDirection { get; init; } = YogaFlexDirection.Row;

	public YogaJustify JustifyContent { get; init; } = YogaJustify.FlexStart;
	public YogaAlign AlignContent { get; init; } = YogaAlign.FlexStart;
	public YogaAlign AlignItems { get; init; } = YogaAlign.Stretch;
	public YogaAlign AlignSelf { get; init; } = YogaAlign.Auto;

	public YogaValue Margin { get; init; } = YogaValue.Undefined();
	public YogaValue MarginLeft { get; init; } = YogaValue.Undefined();
	public YogaValue MarginRight { get; init; } = YogaValue.Undefined();
	public YogaValue MarginTop { get; init; } = YogaValue.Undefined();
	public YogaValue MarginBottom { get; init; } = YogaValue.Undefined();
	public YogaValue MarginVertical { get; init; } = YogaValue.Undefined();
	public YogaValue MarginHorizontal { get; init; } = YogaValue.Undefined();

	public YogaValue Padding { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingLeft { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingRight { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingTop { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingBottom { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingVertical { get; init; } = YogaValue.Undefined();
	public YogaValue PaddingHorizontal { get; init; } = YogaValue.Undefined();

	public YogaValue Width { get; init; } = YogaValue.Auto();
	public YogaValue Height { get; init; } = YogaValue.Auto();
	public YogaValue Left { get; init; } = YogaValue.Undefined();
	public YogaValue Right { get; init; } = YogaValue.Undefined();
	public YogaValue Top { get; init; } = YogaValue.Undefined();
	public YogaValue Bottom { get; init; } = YogaValue.Undefined();

	public YogaValue MinWidth { get; init; } = YogaValue.Undefined();
	public YogaValue MinHeight { get; init; } = YogaValue.Undefined();
	public YogaValue MaxWidth { get; init; } = YogaValue.Undefined();
	public YogaValue MaxHeight { get; init; } = YogaValue.Undefined();

	public float BorderWidth { get; init; }
	public float BorderBottomWidth { get; init; } = float.NaN;
	public float BorderTopWidth { get; init; } = float.NaN;
	public float BorderLeftWidth { get; init; } = float.NaN;
	public float BorderRightWidth { get; init; } = float.NaN;

	public float BorderRadius { get; init; }
	public float BorderBottomRadius { get; init; } = float.NaN;
	public float BorderTopRadius { get; init; } = float.NaN;
	public float BorderLeftRadius { get; init; } = float.NaN;
	public float BorderRightRadius { get; init; } = float.NaN;

	public Color4<Rgba> BorderColor { get; init; }

	public Color4<Rgba> BackgroundColor { get; init; }

	protected static TValue Select<TValue>(TValue first, TValue second, TValue @default) =>
		EqualityComparer<TValue>.Default.Equals(second, @default) ? first : second;

	public static TTarget Merge<TTarget, TSource>(TTarget target, TSource source)
		where TTarget : Style where TSource : TTarget =>
		target with
		{
			Position = Select(target.Position, source.Position, Default.Position),
			Display = Select(target.Display, source.Display, Default.Display),
			Overflow = Select(target.Overflow, source.Overflow, Default.Overflow),
			Flex = Select(target.Flex, source.Flex, Default.Flex),
			AlignContent = Select(target.AlignContent, source.AlignContent, Default.AlignContent),
			JustifyContent = Select(target.JustifyContent, source.JustifyContent, Default.JustifyContent),
			AlignItems = Select(target.AlignItems, source.AlignItems, Default.AlignItems),
			AlignSelf = Select(target.AlignSelf, source.AlignSelf, Default.AlignSelf),
			Margin = Select(target.Margin, source.Margin, Default.Margin),
			MarginLeft = Select(target.MarginLeft, source.MarginLeft, Default.MarginLeft),
			MarginRight = Select(target.MarginRight, source.MarginRight, Default.MarginRight),
			MarginTop = Select(target.MarginTop, source.MarginTop, Default.MarginTop),
			MarginBottom = Select(target.MarginBottom, source.MarginBottom, Default.MarginBottom),
			MarginVertical = Select(target.MarginVertical, source.MarginVertical, Default.MarginVertical),
			MarginHorizontal = Select(target.MarginHorizontal, source.MarginHorizontal, Default.MarginHorizontal),
			Padding = Select(target.Padding, source.Padding, Default.Padding),
			PaddingLeft = Select(target.MarginLeft, source.MarginLeft, Default.MarginLeft),
			PaddingRight = Select(target.MarginRight, source.MarginRight, Default.MarginRight),
			PaddingTop = Select(target.MarginTop, source.MarginTop, Default.MarginTop),
			PaddingBottom = Select(target.MarginBottom, source.MarginBottom, Default.MarginBottom),
			PaddingVertical = Select(target.PaddingVertical, source.PaddingVertical, Default.PaddingVertical),
			PaddingHorizontal = Select(target.PaddingHorizontal, source.PaddingHorizontal,
				Default.PaddingHorizontal),
			Width = Select(target.Width, source.Width, Default.Width),
			Height = Select(target.Height, source.Height, Default.Height),
			Left = Select(target.Left, source.Left, Default.Left),
			Right = Select(target.Right, source.Right, Default.Right),
			Top = Select(target.Top, source.Top, Default.Top),
			Bottom = Select(target.Bottom, source.Bottom, Default.Bottom),
			MinWidth = Select(target.MinWidth, source.MinWidth, Default.MinWidth),
			MinHeight = Select(target.MinHeight, source.MinHeight, Default.MinHeight),
			MaxWidth = Select(target.MaxWidth, source.MaxWidth, Default.MaxWidth),
			MaxHeight = Select(target.MaxHeight, source.MaxHeight, Default.MaxHeight),
			BackgroundColor = Select(target.BackgroundColor, source.BackgroundColor, Default.BackgroundColor),
			BorderWidth = Select(target.BorderWidth, source.BorderWidth, Default.BorderWidth),
			BorderBottomWidth = Select(target.BorderBottomWidth, source.BorderBottomWidth, Default.BorderBottomWidth),
			BorderTopWidth = Select(target.BorderTopWidth, source.BorderTopWidth, Default.BorderTopWidth),
			BorderLeftWidth = Select(target.BorderLeftWidth, source.BorderLeftWidth, Default.BorderLeftWidth),
			BorderRightWidth = Select(target.BorderRightWidth, source.BorderRightWidth, Default.BorderRightWidth),
		};

	protected static string? GetValue(XElement element, string attribute,
		IReadOnlyDictionary<string, string> values)
	{
		var value = element.Attribute(attribute)?.Value;
		if (value is null)
		{
			return null;
		}

		if (value.StartsWith(ValuePrefix) && !value.StartsWith(ValueEscapeSequence))
		{
			return values.TryGetValue(value.Substring(ValuePrefix.Length), out var substitute)
				? substitute
				: value;
		}

		return value;
	}

	protected static float ParseFloat(string? floatString, float defaultValue) =>
		float.TryParse(floatString, out var value) ? value : defaultValue;

	protected static T ParseEnum<T>(string? enumString, T defaultValue) where T : struct =>
		Enum.TryParse<T>(enumString, ignoreCase: true, out var value) ? value : defaultValue;

	protected static YogaValue ParseYogaValue(string? valueString, YogaValue defaultValue)
	{
		if (valueString is null)
		{
			return defaultValue;
		}

		return valueString.ToLowerInvariant() switch
		{
			"auto" => YogaValue.Auto(),
			"undefined" => YogaValue.Undefined(),
			var pct when pct.EndsWith("%") => float.TryParse(pct.AsSpan(start: 0, pct.Length - 1), out var pctVal)
				? YogaValue.Percent(pctVal)
				: defaultValue,
			_ => float.TryParse(valueString, out var val) ? val : defaultValue
		};
	}
}