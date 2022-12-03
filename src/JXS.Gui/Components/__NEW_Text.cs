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

public class __NEW_Text : Component
{
	// Cached resources
	private readonly List<TextRow> textRows;
	private string textContent;
	private bool dirty;

	private Font font;
	private TextLayout layout;

	private TextStyle textStyle;

	public __NEW_Text(Font font, string value)
	{
		Font = font;
		TextContent = value;
		Style = new TextStyle();
		layout = new TextLayout(font);
		textRows = new List<TextRow>();
	}

	public new TextStyle Style
	{
		get => textStyle;
		set
		{
			textStyle = value;
			base.Style = value;
		}
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
			maxTextWidth = CalculatedBounds.Size.X;
			if (maxTextWidth == 0)
			{
				maxTextWidth = Parent?.CalculatedBounds.Size.X ?? 0;
				if (maxTextWidth == 0)
				{
					maxTextWidth = float.PositiveInfinity;
				}
			}
		}

		if (dirty)
		{
			textRows.Clear();
			textRows.AddRange(layout.LineBreak(TextContent,
				maxTextWidth * (font.Atlas.CharacterPixelSize / Style.FontSize), Style.TextBreakStrategy));
			dirty = false;
		}

		var position = CalculatedBounds.Location;
		var textAreaSize = CalculatedBounds.Size;
		var textSize = font.ScalePixelsToFontSize(TextSize, Style.FontSize);
		position.Y += font.ScaleEmToFontSize(font.Metrics.LineHeight, Style.FontSize) * (textRows.Count - 1);

		// Bottom align is default, no need to check for it
		if (Style.TextAlign.HasFlag(TextAlign.Top))
		{
			position.Y += textAreaSize.Y - textSize.Y;
		}
		else if (Style.TextAlign.HasFlag(TextAlign.VerticalCenter))
		{
			position.Y += (textAreaSize.Y - textSize.Y) / 2;
		}

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
				var positionOffset = new Vector2(x: 0, offsetY);
				// Left align is default, no need to check for it
				var emptySpace = textAreaSize.X - font.ScalePixelsToFontSize(row.Size.X, Style.FontSize);
				if (Style.TextAlign.HasFlag(TextAlign.Right))
				{
					positionOffset.X += emptySpace;
				}
				else if (Style.TextAlign.HasFlag(TextAlign.HorizontalCenter))
				{
					positionOffset.X += emptySpace / 2f;
				}

				graphicsProvider.DrawText(Font, row, Style.FontSize, position + positionOffset, Style.FontColor);
				offsetY -= font.ScalePixelsToFontSize(row.Size.Y, Style.FontSize);
			}
		}
	}
}