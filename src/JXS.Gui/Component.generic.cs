using Facebook.Yoga;
using OpenTK.Mathematics;

namespace JXS.Gui;

public abstract class Component<TStyle> : Component where TStyle : Style, new()
{
	/// <summary>
	///     The Style of this component.
	/// </summary>
	public virtual TStyle Style { get; set; } = new();

	public override bool Visible => Style.Display != YogaDisplay.None;

	public override YogaOverflow Overflow => Style.Overflow;

	public override Color4<Rgba> BackgroundColor => Style.BackgroundColor;

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
		Node.FlexDirection = style.FlexDirection;
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
	}
}