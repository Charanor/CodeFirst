using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public partial class Frame
{
	public YogaPositionType Position { get; set; } = YogaPositionType.Relative;
	public YogaDisplay Display { get; set; } = YogaDisplay.Flex;
	public YogaOverflow Overflow { get; set; } = YogaOverflow.Visible;

	public float Flex { get; set; } = YogaConstants.Undefined;
	public float FlexShrink { get; set; } = YogaConstants.Undefined;
	public float FlexGrow { get; set; } = YogaConstants.Undefined;
	public YogaValue FlexBasis { get; set; } = YogaValue.Auto();
	public YogaFlexDirection FlexDirection { get; set; } = YogaFlexDirection.Row;

	public float AspectRatio { get; set; } = YogaConstants.Undefined;

	public YogaJustify JustifyContent { get; set; } = YogaJustify.FlexStart;
	public YogaAlign AlignContent { get; set; } = YogaAlign.FlexStart;
	public YogaAlign AlignItems { get; set; } = YogaAlign.Stretch;
	public YogaAlign AlignSelf { get; set; } = YogaAlign.Auto;

	public YogaValue Margin { get; set; } = YogaValue.Undefined();
	public YogaValue MarginLeft { get; set; } = YogaValue.Undefined();
	public YogaValue MarginRight { get; set; } = YogaValue.Undefined();
	public YogaValue MarginTop { get; set; } = YogaValue.Undefined();
	public YogaValue MarginBottom { get; set; } = YogaValue.Undefined();
	public YogaValue MarginVertical { get; set; } = YogaValue.Undefined();
	public YogaValue MarginHorizontal { get; set; } = YogaValue.Undefined();

	public YogaValue Padding { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingLeft { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingRight { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingTop { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingBottom { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingVertical { get; set; } = YogaValue.Undefined();
	public YogaValue PaddingHorizontal { get; set; } = YogaValue.Undefined();

	public YogaValue Width { get; set; } = YogaValue.Auto();
	public YogaValue Height { get; set; } = YogaValue.Auto();
	public YogaValue Left { get; set; } = YogaValue.Undefined();
	public YogaValue Right { get; set; } = YogaValue.Undefined();
	public YogaValue Top { get; set; } = YogaValue.Undefined();
	public YogaValue Bottom { get; set; } = YogaValue.Undefined();

	public YogaValue MinWidth { get; set; } = YogaValue.Undefined();
	public YogaValue MinHeight { get; set; } = YogaValue.Undefined();
	public YogaValue MaxWidth { get; set; } = YogaValue.Undefined();
	public YogaValue MaxHeight { get; set; } = YogaValue.Undefined();

	public float BorderWidth { get; set; } // Not NaN!
	public float BorderBottomWidth { get; set; } = YogaConstants.Undefined;
	public float BorderTopWidth { get; set; } = YogaConstants.Undefined;
	public float BorderLeftWidth { get; set; } = YogaConstants.Undefined;
	public float BorderRightWidth { get; set; } = YogaConstants.Undefined;

	public float BorderRadius { get; set; } // Not NaN!
	public float BorderTopLeftRadius { get; set; } = YogaConstants.Undefined;
	public float BorderTopRightRadius { get; set; } = YogaConstants.Undefined;
	public float BorderBottomLeftRadius { get; set; } = YogaConstants.Undefined;
	public float BorderBottomRightRadius { get; set; } = YogaConstants.Undefined;

	public FrameSkin FrameSkin { get; set; }

	public Color4<Rgba> BorderColor { get; set; }
	public Color4<Rgba> BackgroundColor { get; set; }

	public virtual void ApplyStyle()
	{
		Node.PositionType = Position;
		Node.Display = Display;
		Node.Overflow = Overflow;
		Node.Flex = Flex;
		Node.FlexBasis = FlexBasis;
		Node.FlexGrow = FlexGrow;
		Node.FlexShrink = FlexShrink;
		Node.FlexDirection = FlexDirection;
		Node.AspectRatio = AspectRatio;
		Node.JustifyContent = JustifyContent;
		Node.AlignContent = AlignContent;
		Node.AlignItems = AlignItems;
		Node.AlignSelf = AlignSelf;
		Node.Margin = Margin;
		Node.MarginLeft = MarginLeft;
		Node.MarginRight = MarginRight;
		Node.MarginTop = MarginTop;
		Node.MarginBottom = MarginBottom;
		Node.MarginVertical = MarginVertical;
		Node.MarginHorizontal = MarginHorizontal;
		Node.Padding = Padding;
		Node.PaddingLeft = PaddingLeft;
		Node.PaddingRight = PaddingRight;
		Node.PaddingTop = PaddingTop;
		Node.PaddingBottom = PaddingBottom;
		Node.PaddingVertical = PaddingVertical;
		Node.PaddingHorizontal = PaddingHorizontal;
		Node.Width = Width;
		Node.Height = Height;
		Node.Left = Left;
		Node.Right = Right;
		Node.Top = Top;
		Node.Bottom = Bottom;
		Node.MinWidth = MinWidth;
		Node.MinHeight = MinHeight;
		Node.MaxWidth = MaxWidth;
		Node.MaxHeight = MaxHeight;
		Node.BorderWidth = BorderWidth;
		Node.BorderBottomWidth = BorderBottomWidth;
		Node.BorderTopWidth = BorderTopWidth;
		Node.BorderLeftWidth = BorderLeftWidth;
		Node.BorderRightWidth = BorderRightWidth;

		foreach (var child in children)
		{
			child.ApplyStyle();
		}
	}
}