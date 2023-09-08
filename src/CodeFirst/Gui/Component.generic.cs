using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public abstract class Component<TStyle> : Component where TStyle : Style, new()
{
	/// <summary>
	///     The Style of this component.
	/// </summary>
	public virtual TStyle Style { get; set; } = new();

	public override bool Visible => Style.Display != YogaDisplay.None;

	public override YogaOverflow Overflow => Style.Overflow;

	public override Color4<Rgba> BackgroundColor => Style.BackgroundColor;

	public override (float left, float right, float top, float bottom) BorderSize => (
		NaNSwitch(Style.BorderLeftWidth, Style.BorderWidth),
		NaNSwitch(Style.BorderRightWidth, Style.BorderWidth),
		NaNSwitch(Style.BorderTopWidth, Style.BorderWidth),
		NaNSwitch(Style.BorderBottomWidth, Style.BorderWidth)
	);

	public override (float topLeft, float topRight, float bottomLeft, float bottomRight) BorderRadii => (
		NaNSwitch(Style.BorderTopLeftRadius, Style.BorderRadius),
		NaNSwitch(Style.BorderTopRightRadius, Style.BorderRadius),
		NaNSwitch(Style.BorderBottomLeftRadius, Style.BorderRadius),
		NaNSwitch(Style.BorderBottomRightRadius, Style.BorderRadius)
	);

	public override Color4<Rgba> BorderColor => Style.BorderColor;

	public override void ApplyStyle()
	{
		ApplyStyle(Style);
	}

	protected void ApplyStyle(Style style)
	{
		Node.PositionType = style.Position;
		Node.Display = style.Display;
		Node.Overflow = style.Overflow;
		Node.Flex = style.Flex;
		Node.FlexBasis = style.FlexBasis;
		Node.FlexGrow = style.FlexGrow;
		Node.FlexShrink = style.FlexShrink;
		Node.FlexDirection = style.FlexDirection;
		Node.AspectRatio = style.AspectRatio;
		Node.JustifyContent = style.JustifyContent;
		Node.AlignContent = style.AlignContent;
		Node.AlignItems = style.AlignItems;
		Node.AlignSelf = style.AlignSelf;
		Node.Margin = style.Margin;
		Node.MarginLeft = style.MarginLeft;
		Node.MarginRight = style.MarginRight;
		Node.MarginTop = style.MarginTop;
		Node.MarginBottom = style.MarginBottom;
		Node.MarginVertical = style.MarginVertical;
		Node.MarginHorizontal = style.MarginHorizontal;
		Node.Padding = style.Padding;
		Node.PaddingLeft = style.PaddingLeft;
		Node.PaddingRight = style.PaddingRight;
		Node.PaddingTop = style.PaddingTop;
		Node.PaddingBottom = style.PaddingBottom;
		Node.PaddingVertical = style.PaddingVertical;
		Node.PaddingHorizontal = style.PaddingHorizontal;
		Node.Width = style.Width;
		Node.Height = style.Height;
		Node.Left = style.Left;
		Node.Right = style.Right;
		Node.Top = style.Top;
		Node.Bottom = style.Bottom;
		Node.MinWidth = style.MinWidth;
		Node.MinHeight = style.MinHeight;
		Node.MaxWidth = style.MaxWidth;
		Node.MaxHeight = style.MaxHeight;
		Node.BorderWidth = style.BorderWidth;
		Node.BorderBottomWidth = style.BorderBottomWidth;
		Node.BorderTopWidth = style.BorderTopWidth;
		Node.BorderLeftWidth = style.BorderLeftWidth;
		Node.BorderRightWidth = style.BorderRightWidth;
	}

	private static float NaNSwitch(float value, float fallback) => float.IsNaN(value) ? fallback : value;
}