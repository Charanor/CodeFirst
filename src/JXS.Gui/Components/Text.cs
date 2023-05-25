using System.Xml.Linq;
using Facebook.Yoga;
using JXS.Graphics.Text;
using JXS.Graphics.Text.Layout;
using OpenTK.Mathematics;

namespace JXS.Gui.Components;

[Flags]
public enum TextAlign
{
	Left = 1 << 0,
	Right = 1 << 1,
	Top = 1 << 2,
	Bottom = 1 << 3,
	HorizontalCenter = 1 << 4,
	VerticalCenter = 1 << 5,
	TopLeft = Top | Left,
	TopRight = Top | Right,
	BottomLeft = Bottom | Left,
	BottomRight = Bottom | Right,
	Middle = HorizontalCenter | VerticalCenter,
	TopCenter = Top | HorizontalCenter,
	BottomCenter = Bottom | HorizontalCenter,
	LeftCenter = Left | VerticalCenter,
	RightCenter = Right | VerticalCenter
}

public record TextStyle : Style
{
	private static readonly TextStyle Default = new();

	public TextStyle()
	{
	}

	public TextStyle(XElement element, IReadOnlyDictionary<string, string> values) : base(element, values)
	{
		FontSize = ParseUtils.ParseInt(GetValue(element, nameof(FontSize), values), FontSize);
		FontColor = ParseUtils.ParseColor(GetValue(element, nameof(FontColor), values), FontColor);
		TextAlign = ParseEnum(GetValue(element, nameof(TextAlign), values), TextAlign);
		TextBreakStrategy = ParseEnum(GetValue(element, nameof(TextBreakStrategy), values), TextBreakStrategy);
	}

	public int FontSize { get; init; } = 18;
	public Color4<Rgba> FontColor { get; init; } = Color4.Black;
	public TextAlign TextAlign { get; init; } = TextAlign.TopLeft;
	public TextBreakStrategy TextBreakStrategy { get; init; } = TextBreakStrategy.Whitespace;

	public new static TTarget Merge<TTarget, TSource>(TTarget target, TSource source)
		where TTarget : TextStyle where TSource : TTarget =>
		Style.Merge(target, source) with
		{
			FontSize = Select(target.FontSize, source.FontSize, Default.FontSize),
			FontColor = Select(target.FontColor, source.FontColor, Default.FontColor),
			TextAlign = Select(target.TextAlign, source.TextAlign, Default.TextAlign),
			TextBreakStrategy = Select(target.TextBreakStrategy, source.TextBreakStrategy, Default.TextBreakStrategy)
		};
}

public class Text : Component<TextStyle>
{
	private readonly List<TextRow> textRows;

	private Font font;

	private TextLayout layout;
	// private TextStyle style = new();

	private bool dirty = true;

	private string textContent;

	public Text(Font font, string value)
	{
		textRows = new List<TextRow>();
		Font = this.font = font;
		TextContent = textContent = value;
		layout ??= new TextLayout(font);
	}

	public override TextStyle Style { get; set; } = new();

	public Font Font
	{
		get => font;
		set
		{
			if (font == value)
			{
				return;
			}

			font = value;
			layout = new TextLayout(font);
			dirty = true;
		}
	}

	public string TextContent
	{
		get => textContent;
		set
		{
			if (textContent == value)
			{
				return;
			}

			textContent = value;
			dirty = true;
		}
	}

	public Vector2 TextSize => dirty ? new Vector2(float.NaN, float.NaN) : layout.CalculateTextSize(textRows);

	public override void Draw(IGraphicsProvider graphicsProvider)
	{
		base.Draw(graphicsProvider);

		var maxTextWidth = Node.LayoutWidth - Node.LayoutPaddingLeft - Node.LayoutPaddingRight;
		if (maxTextWidth == 0)
		{
			maxTextWidth = TransformedBounds.Size.X;
			if (maxTextWidth == 0)
			{
				maxTextWidth = Parent?.TransformedBounds.Size.X ?? 0;
				if (maxTextWidth == 0)
				{
					maxTextWidth = float.PositiveInfinity;
				}
			}
		}

		if (dirty)
		{
			textRows.Clear();
			var rows = TextContent
				.Split('\n')
				.SelectMany(hardRow =>
					layout.LineBreak(hardRow, maxTextWidth * (font.Atlas.CharacterPixelSize / Style.FontSize),
						Style.TextBreakStrategy));
			textRows.AddRange(rows);
			dirty = false;
		}

		var textSize = font.ScalePixelsToFontSize(TextSize, Style.FontSize);

		var textAreaSize = TransformedBounds.Size;
		textAreaSize.X = textAreaSize.X <= 0 ? maxTextWidth : MathF.Min(textAreaSize.X, maxTextWidth);
		var position = TransformedBounds.Location;
		var emptyVerticalSpace = textAreaSize.Y - textSize.Y;
		position.Y += Style.TextAlign switch
		{
			var align when align.HasFlag(TextAlign.Bottom) => emptyVerticalSpace,
			var align when align.HasFlag(TextAlign.VerticalCenter) => emptyVerticalSpace / 2f,
			_ => 0 // Top align is default, no need to check for it
		};

		if (Style.Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			DrawText();
			graphicsProvider.EndOverflow();
		}
		else
		{
			DrawText();
		}

		void DrawText()
		{
			var offsetY = 0f;
			foreach (var row in textRows)
			{
				var emptyHorizontalSpace = textAreaSize.X - font.ScalePixelsToFontSize(row.Size.X, Style.FontSize);
				var offsetX = Style.TextAlign switch
				{
					var align when align.HasFlag(TextAlign.Right) => emptyHorizontalSpace,
					var align when align.HasFlag(TextAlign.HorizontalCenter) => emptyHorizontalSpace / 2f,
					_ => 0 // Left align is default, no need to check for it
				};

				var positionOffset = new Vector2(offsetX, offsetY);
				graphicsProvider.DrawText(Font, row, Style.FontSize, position + positionOffset, Style.FontColor);
				offsetY += font.ScaleEmToFontSize(font.Metrics.LineHeight, Style.FontSize);
			}
		}
	}
}