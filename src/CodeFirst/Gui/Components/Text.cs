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
	private readonly List<TextRow> textRows;

	private Font font;
	private TextLayout layout;
	private bool dirty = true;
	private string textContent;

	public Text(Font font, string value = "")
	{
		textRows = new List<TextRow>();
		Font = this.font = font;
		TextContent = textContent = value;
		layout ??= new TextLayout(font);
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
			dirty = true;
		}
	}

	public int FontSize { get; init; } = 18;
	public Color4<Rgba> FontColor { get; init; } = Color4.Black;
	public TextAlign TextAlign { get; init; } = TextAlign.TopLeft;
	public TextBreakStrategy TextBreakStrategy { get; init; } = TextBreakStrategy.Whitespace;

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

	public override void ApplyStyle()
	{
		base.ApplyStyle();

		if (dirty)
		{
			var maxTextWidth = CalculateMaxTextWidth();
			textRows.Clear();
			var rows = TextContent
				.Split('\n')
				.SelectMany(hardRow =>
					layout.LineBreak(hardRow, maxTextWidth * (font.Atlas.CharacterPixelSize / FontSize),
						TextBreakStrategy));
			textRows.AddRange(rows);
			dirty = false;
		}

		var textSize = font.ScalePixelsToFontSize(TextSize, FontSize);

		if (MinWidth.Unit is YogaUnit.Auto or YogaUnit.Undefined)
		{
			Node.MinWidth = textSize.X;
		}

		if (MinHeight.Unit is YogaUnit.Auto or YogaUnit.Undefined)
		{
			Node.MinHeight = textSize.Y;
		}
	}

	protected override void AfterDrawBackground(IGraphicsProvider graphicsProvider)
	{
		base.AfterDrawBackground(graphicsProvider);

		var maxTextWidth = CalculateMaxTextWidth();
		if (dirty)
		{
			textRows.Clear();
			var rows = TextContent
				.Split('\n')
				.SelectMany(hardRow =>
					layout.LineBreak(hardRow, maxTextWidth * (font.Atlas.CharacterPixelSize / FontSize),
						TextBreakStrategy));
			textRows.AddRange(rows);
			dirty = false;
		}

		var textSize = font.ScalePixelsToFontSize(TextSize, FontSize);

		var textAreaSize = TransformedBounds.Size;
		textAreaSize.X = textAreaSize.X <= 0 ? maxTextWidth : MathF.Min(textAreaSize.X, maxTextWidth);
		var position = TransformedBounds.Location;
		var emptyVerticalSpace = textAreaSize.Y - textSize.Y;
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
}