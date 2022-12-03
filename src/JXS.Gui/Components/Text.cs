// using System.Text;
// using System.Xml.Linq;
// using Facebook.Yoga;
// using JXS.Graphics.Text.Layout;
// using OpenTK.Mathematics;
//
// namespace JXS.Gui.Components;
//
// [Flags]
// public enum TextAlign
// {
// 	Left = 1 << 0,
// 	Right = 1 << 1,
// 	Top = 1 << 2,
// 	Bottom = 1 << 3,
// 	HorizontalCenter = 1 << 4,
// 	VerticalCenter = 1 << 5,
// 	TopLeft = Top | Left,
// 	TopRight = Top | Right,
// 	BottomLeft = Bottom | Left,
// 	BottomRight = Bottom | Right,
// 	Middle = HorizontalCenter | VerticalCenter,
// 	TopCenter = Top | HorizontalCenter,
// 	BottomCenter = Bottom | HorizontalCenter,
// 	LeftCenter = Left | VerticalCenter,
// 	RightCenter = Right | VerticalCenter
// }
//
// public record TextStyle : Style
// {
// 	private static readonly TextStyle Default = new();
//
// 	public TextStyle()
// 	{
// 	}
//
// 	public TextStyle(XElement element, IReadOnlyDictionary<string, string> values) : base(element, values)
// 	{
// 		FontSize = ParseUtils.ParseInt(GetValue(element, nameof(FontSize), values), FontSize);
// 		FontColor = ParseUtils.ParseColor(GetValue(element, nameof(FontColor), values), FontColor);
// 		TextAlign = ParseEnum(GetValue(element, nameof(TextAlign), values), TextAlign);
// 		TextBreakStrategy = ParseEnum(GetValue(element, nameof(TextBreakStrategy), values), TextBreakStrategy);
// 	}
//
// 	public int FontSize { get; init; } = 18;
// 	public Color4<Rgba> FontColor { get; init; } = Color4.Black;
// 	public TextAlign TextAlign { get; init; } = TextAlign.TopLeft;
// 	public TextBreakStrategy TextBreakStrategy { get; init; } = TextBreakStrategy.Whitespace;
//
// 	public new static TTarget Merge<TTarget, TSource>(TTarget target, TSource source)
// 		where TTarget : TextStyle where TSource : TTarget =>
// 		Style.Merge(target, source) with
// 		{
// 			FontSize = Select(target.FontSize, source.FontSize, Default.FontSize),
// 			FontColor = Select(target.FontColor, source.FontColor, Default.FontColor),
// 			TextAlign = Select(target.TextAlign, source.TextAlign, Default.TextAlign),
// 			TextBreakStrategy = Select(target.TextBreakStrategy, source.TextBreakStrategy, Default.TextBreakStrategy)
// 		};
// }
//
// public class Text : Component
// {
// 	private const string SPLIT_CHARACTER = " ";
//
// 	private Vector2 textSize;
// 	private string value = "";
// 	private bool dirty;
//
// 	public Text(string value, TextStyle? style, string? id, IInputProvider inputProvider) : base(style, id,
// 		inputProvider)
// 	{
// 		Value = value;
// 		Style = style ?? new TextStyle();
// 	}
//
// 	public string Value
// 	{
// 		get => value;
// 		set
// 		{
// 			if (Value == value)
// 			{
// 				return;
// 			}
//
// 			this.value = value;
// 			dirty = true;
// 		}
// 	}
//
// 	public new TextStyle Style { get; set; }
//
// 	public override void Draw(IGraphicsProvider graphicsProvider)
// 	{
// 		base.Draw(graphicsProvider);
// 		var size = CalculatedBounds.Size;
//
// 		if (dirty)
// 		{
// 			var lines =
// 				Style.TextBreakStrategy != TextBreakStrategy.None
// 					? WrapText(Value, size.X, measureText: str => graphicsProvider.MeasureText(Style.FontSize, str).X)
// 						.ToList()
// 					: new List<string> { value };
// 			Value = string.Join(separator: "\n", lines);
// 			textSize = lines.Select(s => graphicsProvider.MeasureText(Style.FontSize, s)).MaxBy(s => s.X);
//
// 			//Console.WriteLine($"Text {Value} measured to {textSize} (font size: {Style.FontSize})");
// 			dirty = false;
// 		}
// 		//if (Style.MinWidth.Unit == YogaUnit.Undefined || textSize.X > Style.MinWidth.Value)
// 		//    Style = Style with {MinWidth = textSize.X};
// 		//if (Style.MinHeight.Unit == YogaUnit.Undefined || textSize.Y > Style.MinHeight.Value)
// 		//    Style = Style with {MinHeight = textSize.Y};
//
// 		var position = CalculatedBounds.Min;
//
// 		// Left align is default, no need to check for it
// 		if (Style.TextAlign.HasFlag(TextAlign.Right))
// 		{
// 			position.X += size.X - textSize.X;
// 		}
// 		else if (Style.TextAlign.HasFlag(TextAlign.HorizontalCenter))
// 		{
// 			position.X += (size.X - textSize.X) / 2;
// 		}
//
// 		// Top align is default, no need to check for it
// 		if (Style.TextAlign.HasFlag(TextAlign.Bottom))
// 		{
// 			position.Y += size.Y - textSize.Y;
// 		}
// 		else if (Style.TextAlign.HasFlag(TextAlign.VerticalCenter))
// 		{
// 			position.Y += (size.Y - textSize.Y) / 2;
// 		}
//
// 		var maxTextWidth = Node.LayoutWidth - Node.LayoutPaddingLeft - Node.LayoutPaddingRight;
// 		var log = false; //Parent?.Id == "Input"
// 		if (Style.Overflow == YogaOverflow.Hidden)
// 		{
// 			var scissor = CalculatedBounds.Floor();
// 			graphicsProvider.AddScissor(scissor);
// 			graphicsProvider.DrawText(Style.FontSize, Value, position, Style.FontColor, maxTextWidth, log);
// 			graphicsProvider.RemoveScissor(scissor);
// 		}
// 		else
// 		{
// 			graphicsProvider.DrawText(Style.FontSize, Value, position, Style.FontColor, maxTextWidth, log);
// 		}
// 	}
//
// 	public override void ApplyStyle() =>
// 		ApplyStyle(TextStyle.Merge(new TextStyle { Width = textSize.X, Height = textSize.Y }, Style));
//
// 	private static IEnumerable<string> WrapText(string text, double maxWidth, Func<string, double> measureText)
// 	{
// 		var originalLines = text.Split(SPLIT_CHARACTER).Select(str => str + SPLIT_CHARACTER);
// 		var wrappedLines = new List<string>();
//
// 		var actualLine = new StringBuilder();
// 		double actualWidth = 0;
//
// 		foreach (var line in originalLines)
// 		{
// 			var lineWidth = measureText(line);
// 			actualWidth += lineWidth;
//
// 			if (actualWidth <= maxWidth)
// 			{
// 				actualLine.Append(line);
// 				continue;
// 			}
//
// 			wrappedLines.Add(actualLine.ToString());
// 			actualLine.Clear();
// 			actualLine.Append(line);
// 			actualWidth = lineWidth;
// 		}
//
// 		if (actualLine.Length > 0)
// 		{
// 			wrappedLines.Add(actualLine.ToString());
// 		}
//
// 		return wrappedLines;
// 	}
// }