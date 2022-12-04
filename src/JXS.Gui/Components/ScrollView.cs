using Facebook.Yoga;
using OpenTK.Mathematics;

namespace JXS.Gui.Components;

public class ScrollView : View
{
	private readonly View contentContainerContainer;
	private readonly View contentContainer;
	private readonly View verticalKnob;
	private readonly View horizontalKnob;

	private readonly Pressable verticalScrollBar;
	private readonly Pressable horizontalScrollBar;

	private Vector2 prevMousePos;
	private Vector2 knobOffset;
	private bool scrollingVertical;
	private bool scrollingHorizontal;

	private Vector2 actualContentSize;

	public ScrollView(string? id = default, Style? style = default, Style? contentContainerStyle = default) : base(id, style)
	{
		// TODO: Move this to style object
		const float knobSize = 15;
		contentContainerStyle ??= new Style();
		var verticalContent = new View(id: null, new Style
		{
			Flex = 1,
			FlexDirection = YogaFlexDirection.Column,
			JustifyContent = YogaJustify.SpaceBetween
		});
		contentContainerContainer = new View(id: null, new Style
		{
			Flex = 1,
			Overflow = YogaOverflow.Hidden
		});
		contentContainer = new View(id: "Scroll_ContentContainer", contentContainerStyle);
		verticalScrollBar = new Pressable(id: null, style: new Style
		{
			Width = knobSize
		});
		verticalKnob = new View(id: null, new Style
		{
			BackgroundColor = Color4.Lightblue,
			Height = YogaValue.Percent(0),
			Width = knobSize
		});
		horizontalScrollBar = new Pressable(id: null, style: new Style
		{
			Height = knobSize
		});
		horizontalKnob = new View(id: null, new Style
		{
			BackgroundColor = Color4.Lightblue,
			Width = YogaValue.Percent(0),
			Height = knobSize
		});

		contentContainerContainer.AddChild(contentContainer);
		verticalContent.AddChild(contentContainerContainer);
		verticalContent.AddChild(horizontalScrollBar);
		horizontalScrollBar.AddChild(horizontalKnob);
		verticalScrollBar.AddChild(verticalKnob);

		base.AddChild(verticalContent);
		base.AddChild(verticalScrollBar);

		horizontalScrollBar.OnPressDown += (_, args) =>
		{
			if (args.PressEvent != GuiInputAction.Primary)
			{
				return;
			}

			scrollingHorizontal = true;
			if (args.Component != horizontalKnob)
				// We didn't touch the knob, scroll the knob to where we pressed
			{
				knobOffset.X = args.Position!.Value.X - horizontalKnob.CalculatedBounds.Width / 2;
			}
		};
		verticalScrollBar.OnPressDown += (_, args) =>
		{
			if (args.PressEvent != GuiInputAction.Primary)
			{
				return;
			}

			scrollingVertical = true;
			if (args.Component != verticalKnob)
				// We didn't touch the knob, scroll the knob to where we pressed
			{
				knobOffset.Y = args.Position!.Value.Y - verticalKnob.CalculatedBounds.Height / 2;
			}
		};

		knobOffset = new Vector2();
	}

	private Vector2 ContentContainerSize => contentContainerContainer.CalculatedBounds.Size;

	public override void AddChild(Component component) => contentContainer.AddChild(component);

	public override void Update(float delta)
	{
		base.Update(delta);

		if (contentContainer.Node.HasNewLayout)
		{
			// Re-calculate content size
			var tmp = new YogaNode();
			tmp.CopyStyle(contentContainer.Node);
			CopyChildren(contentContainer.Node, tmp);
			tmp.CalculateLayout();
			actualContentSize = new Vector2(tmp.LayoutWidth, tmp.LayoutHeight);

			static void CopyChildren(YogaNode from, YogaNode to)
			{
				foreach (var child in from)
				{
					var newChild = new YogaNode();
					CopyChildren(child, newChild);

					newChild.CopyStyle(child);
					to.AddChild(newChild);
				}
			}

			contentContainer.Node.MarkLayoutSeen();
		}

		var visiblePercents = ContentContainerSize / actualContentSize;
		horizontalKnob.Style = horizontalKnob.Style with
		{
			Width = YogaValue.Percent(Math.Min(val1: 1, visiblePercents.X) * 100),
			Left = knobOffset.X
		};
		verticalKnob.Style = verticalKnob.Style with
		{
			Height = YogaValue.Percent(Math.Min(val1: 1, visiblePercents.Y) * 100),
			Top = knobOffset.Y
		};

		if (InputProvider?.JustReleased(GuiInputAction.Primary) ?? false)
		{
			scrollingHorizontal = false;
			scrollingVertical = false;
		}

		var mousePos = InputProvider?.MousePosition ?? Vector2.Zero;
		var mousePosDelta = mousePos - prevMousePos;
		if (scrollingHorizontal)
		{
			knobOffset.X += mousePosDelta.X;
		}

		if (scrollingVertical)
		{
			knobOffset.Y += mousePosDelta.Y;
		}

		prevMousePos = mousePos;

		var maxKnobOffsets = new Vector2(
			horizontalScrollBar.CalculatedBounds.Width - horizontalKnob.CalculatedBounds.Width,
			verticalScrollBar.CalculatedBounds.Height - verticalKnob.CalculatedBounds.Height
		);
		if (!float.IsNaN(maxKnobOffsets.X))
		{
			knobOffset.X = Math.Min(Math.Max(knobOffset.X, val2: 0), maxKnobOffsets.X);
		}

		if (!float.IsNaN(maxKnobOffsets.Y))
		{
			knobOffset.Y = Math.Min(Math.Max(knobOffset.Y, val2: 0), maxKnobOffsets.Y);
		}

		var maxContentOffsets = actualContentSize - ContentContainerSize;
		var offset = knobOffset / maxKnobOffsets * maxContentOffsets;
		contentContainer.Style = contentContainer.Style with
		{
			Left = -offset.X,
			Top = -offset.Y
		};
	}
}