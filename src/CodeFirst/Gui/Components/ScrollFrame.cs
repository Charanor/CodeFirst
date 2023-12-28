using CodeFirst.Gui.Events;
using CodeFirst.Utils.Events;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui.Components;

public class ScrollFrame : Frame
{
	public ScrollFrame()
	{
		ContentContainer = new Frame();
	}

	public Frame ContentContainer { get; }

	/// <summary>
	///     How much this frame's content is currently scrolled.
	/// </summary>
	/// <seealso cref="ScrollTo(OpenTK.Mathematics.Vector2)" />
	/// <seealso cref="ScrollBy(OpenTK.Mathematics.Vector2)" />
	public Vector2 ScrollOffset { get; private set; }

	/// <summary>
	///     How much this frame's content is currently scrolled relative to the maximum scroll amount.
	/// </summary>
	/// <seealso cref="ScrollTo(OpenTK.Mathematics.Vector2)" />
	/// <seealso cref="ScrollBy(OpenTK.Mathematics.Vector2)" />
	public Vector2 NormalizedScrollOffset
	{
		get
		{
			var maxScrollOffset = CalculateMaxScrollOffset();
			return new Vector2(
				maxScrollOffset.X == 0 ? 0 : ScrollOffset.X / maxScrollOffset.X,
				maxScrollOffset.Y == 0 ? 0 : ScrollOffset.Y / maxScrollOffset.Y
			);
		}
		private set
		{
			var maxScrollOffset = CalculateMaxScrollOffset();
			ScrollOffset = Vector2.ComponentMin(value, Vector2.One) * maxScrollOffset;
		}
	}

	public event EventHandler<Frame, ScrollEvent>? OnScroll;

	public void ScrollBy(Vector2 delta) => ScrollBy(delta.X, delta.Y);
	public void ScrollBy(float vertical) => ScrollBy(horizontal: 0, vertical);

	public void ScrollBy(float horizontal, float vertical) =>
		ScrollTo(ScrollOffset.X + horizontal, ScrollOffset.Y + vertical);

	public void ScrollTo(Vector2 point) => ScrollTo(point.X, point.Y);
	public void ScrollTo(float y) => ScrollTo(x: 0, y);

	public void ScrollTo(float x, float y)
	{
		var maxScrollOffset = CalculateMaxScrollOffset();
		var newScrollOffset = Vector2.ComponentMin((x, y), maxScrollOffset);
		var delta = newScrollOffset - ScrollOffset;
		if (delta == Vector2.Zero)
		{
			return;
		}

		var evt = new ScrollEvent
		{
			Delta = delta,
			Offset = newScrollOffset,
			NormalizedOffset = new Vector2(
				maxScrollOffset.X == 0 ? 0 : newScrollOffset.X / maxScrollOffset.X,
				maxScrollOffset.Y == 0 ? 0 : newScrollOffset.Y / maxScrollOffset.Y
			)
		};

		if (OnScroll != null)
		{
			foreach (var handler in OnScroll.GetInvocationList().OfType<EventHandler<Frame, ScrollEvent>>())
			{
				handler(this, evt);
				if (evt.Cancelled)
				{
					break;
				}
			}
		}

		if (!evt.DefaultPrevented)
		{
			HandleScroll(newScrollOffset);
		}
	}

	private void UpdateScrollOffset()
	{
		var maxScrollOffset = CalculateMaxScrollOffset();
		ScrollOffset = Vector2.ComponentMin(ScrollOffset, maxScrollOffset);
	}

	protected virtual void HandleScroll(Vector2 offset)
	{
		ScrollOffset = offset;
	}

	private Vector2 CalculateMaxScrollOffset()
	{
		if (Parent == null)
		{
			return Vector2.Zero;
		}

		if (Overflow != YogaOverflow.Scroll)
		{
			return Vector2.Zero;
		}

		// TODO: Check if maybe we should use TranslatedBounds here
		var parentBounds = Parent.CalculatedBounds;
		var selfBounds = CalculatedBounds;
		var contentBounds = parentBounds.Intersected(selfBounds);
		return selfBounds.Size - contentBounds.Size;
	}

	public override void AddChild(Frame frame) => ContentContainer.AddChild(frame);
	public override bool RemoveChild(Frame frame) => ContentContainer.RemoveChild(frame);
	public override void RemoveAllChildren() => ContentContainer.RemoveAllChildren();
	public override IEnumerable<Frame> GetChildren() => ContentContainer.GetChildren();
}