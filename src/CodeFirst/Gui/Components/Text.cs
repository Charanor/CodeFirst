using CodeFirst.Graphics.Text;
using CodeFirst.Graphics.Text.Layout;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui.Components;

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

public class Text : Frame
{
	private const int DEFAULT_FONT_SIZE = 18;
	private const TextBreakStrategy DEFAULT_BREAK_STRATEGY = TextBreakStrategy.Whitespace;

	private readonly List<TextRow> textRows;

	private Font font;
	private TextLayout layout;

	private string textContent;
	private int fontSize = DEFAULT_FONT_SIZE;
	private TextBreakStrategy textBreakStrategy = DEFAULT_BREAK_STRATEGY;

	public Text(Font font, string value = "")
	{
		textRows = new List<TextRow>();
		Font = this.font = font;
		TextContent = textContent = value;
		layout ??= new TextLayout(font);
		Node.SetMeasureFunction(Measure);
	}

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
			Node.MarkDirty();
		}
	}

	public int FontSize
	{
		get => fontSize;
		set
		{
			if (fontSize == value)
			{
				return;
			}

			fontSize = value;
			Node.MarkDirty();
		}
	}

	public Color4<Rgba> FontColor { get; set; } = Color4.Black;
	public TextAlign TextAlign { get; set; } = TextAlign.TopLeft;

	public TextBreakStrategy TextBreakStrategy
	{
		get => textBreakStrategy;
		set
		{
			if (textBreakStrategy == value)
			{
				return;
			}

			textBreakStrategy = value;
			Node.MarkDirty();
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
			Node.MarkDirty();
		}
	}

	public Vector2 TextSize { get; private set; }

	private float CalculateMaxTextWidth()
	{
		var maxTextWidth = Node.LayoutWidth - Node.LayoutPaddingLeft - Node.LayoutPaddingRight;
		if (maxTextWidth != 0)
		{
			return maxTextWidth;
		}

		maxTextWidth = TransformedBounds.Size.X;
		if (maxTextWidth != 0)
		{
			return maxTextWidth;
		}

		maxTextWidth = Parent?.TransformedBounds.Size.X ?? 0;
		if (maxTextWidth == 0)
		{
			maxTextWidth = float.PositiveInfinity;
		}

		return maxTextWidth;
	}

	protected override void OnNewLayout()
	{
		base.OnNewLayout();

		var maxTextWidth = CalculateMaxTextWidth();
		textRows.Clear();
		textRows.AddRange(TextContent
			.Split('\n')
			.SelectMany(hardRow => layout.LineBreak(
				hardRow,
				maxTextWidth * (font.Atlas.CharacterPixelSize / FontSize),
				TextBreakStrategy
			)));
		TextSize = font.ScalePixelsToFontSize(layout.CalculateTextSize(textRows), FontSize);
	}

	protected override void AfterDrawBackground(IGraphicsProvider graphicsProvider)
	{
		base.AfterDrawBackground(graphicsProvider);

		var maxTextWidth = CalculateMaxTextWidth();

		var textAreaSize = TransformedBounds.Size;
		textAreaSize.X = textAreaSize.X <= 0 ? maxTextWidth : MathF.Min(textAreaSize.X, maxTextWidth);
		var position = TransformedBounds.Location;
		var emptyVerticalSpace = textAreaSize.Y - TextSize.Y;
		position.Y += TextAlign switch
		{
			var align when align.HasFlag(TextAlign.Bottom) => emptyVerticalSpace,
			var align when align.HasFlag(TextAlign.VerticalCenter) => emptyVerticalSpace / 2f,
			_ => 0 // Top align is default, no need to check for it
		};

		// Draw text
		var offsetY = 0f;
		foreach (var row in textRows)
		{
			var emptyHorizontalSpace = textAreaSize.X - font.ScalePixelsToFontSize(row.Size.X, FontSize);
			var offsetX = TextAlign switch
			{
				var align when align.HasFlag(TextAlign.Right) => emptyHorizontalSpace,
				var align when align.HasFlag(TextAlign.HorizontalCenter) => emptyHorizontalSpace / 2f,
				_ => 0 // Left align is default, no need to check for it
			};

			var positionOffset = new Vector2(offsetX, offsetY);
			graphicsProvider.DrawText(Font, row, FontSize, position + positionOffset, FontColor);
			offsetY += font.ScaleEmToFontSize(font.Metrics.LineHeight, FontSize);
		}
	}

	private YogaSize Measure(YogaNode node,
		float width, YogaMeasureMode widthMode,
		float height, YogaMeasureMode heightMode)
	{
		var maxWidth = widthMode == YogaMeasureMode.Undefined ? float.PositiveInfinity : width;
		var textSize = font.ScalePixelsToFontSize(
			layout.CalculateTextSize(textContent, maxWidth, TextBreakStrategy),
			FontSize
		);

		return new YogaSize
		{
			width = widthMode switch
			{
				YogaMeasureMode.Exactly => width,
				YogaMeasureMode.AtMost => Math.Min(textSize.X, width),
				_ => textSize.X
			},
			height = heightMode switch
			{
				YogaMeasureMode.Exactly => height,
				YogaMeasureMode.AtMost => Math.Min(textSize.Y, height),
				_ => textSize.Y
			}
		};
	}
}