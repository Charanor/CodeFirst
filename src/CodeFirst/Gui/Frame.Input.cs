using CodeFirst.Gui.Events;
using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public partial class Frame
{
	private ExplicitInputState explicitEnableInput = ExplicitInputState.NotSet;

	/// <summary>
	///     If input events can hit this frame. Defaults to <c>false</c>.
	/// </summary>
	/// <remarks>
	///     If <c>false</c> input events will still propagate to this frame's children unless
	///     <see cref="PropagateInputsToChildren" /> is also <c>false</c>.
	/// </remarks>
	public bool EnableInput
	{
		get => explicitEnableInput switch
		{
			ExplicitInputState.Enabled => true,
			ExplicitInputState.Disabled => false,
			_ => OnPressIn != null || OnPressOut != null || OnFullPress != null || OnCursorEnter != null ||
			     OnCursorExit != null
		};
		set => explicitEnableInput = value ? ExplicitInputState.Enabled : ExplicitInputState.Disabled;
	}

	/// <summary>
	///     If input events that hit this frame should propagate to its children. If <c>true</c> input events will
	///     propagate to this frame's children even if <see cref="EnableInput" /> if <c>false</c>. Defaults to
	///     <c>true</c>.
	/// </summary>
	public bool PropagateInputsToChildren { get; set; } = true;

	/// <summary>
	///     <c>true</c> if this frame is currently being hovered over. <c>false</c> otherwise.
	/// </summary>
	public bool IsHovered { get; private set; }

	public event EventHandler<Frame, UiActionEvent>? OnPressIn;
	public event EventHandler<Frame, UiActionEvent>? OnPressOut;
	public event EventHandler<Frame, UiActionEvent>? OnFullPress;

	public event EventHandler<Frame, CursorEvent>? OnCursorEnter;
	public event EventHandler<Frame, CursorEvent>? OnCursorExit;

	/// <returns>
	///     <c>true</c> if this frame is currently being pressed <b>down</b> on.
	/// </returns>
	public bool IsPressed(UiAction action) => pressedActions.Contains(action);

	private void SetPressed(UiAction action, bool pressed)
	{
		if (pressed)
		{
			pressedActions.Add(action);
		}
		else
		{
			pressedActions.Remove(action);
		}
	}

	private void ClearPressed() => pressedActions.Clear();

	internal void CursorEnter()
	{
		var evt = new CursorEvent();
		if (OnCursorEnter != null)
		{
			foreach (var handler in OnCursorEnter.GetInvocationList().OfType<EventHandler<Frame, CursorEvent>>())
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
			HandleCursorEnter();
		}

		// This should not be default-prevented
		IsHovered = true;
	}

	internal void CursorExit()
	{
		var evt = new CursorEvent();
		if (OnCursorExit != null)
		{
			foreach (var handler in OnCursorExit.GetInvocationList().OfType<EventHandler<Frame, CursorEvent>>())
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
			HandleCursorExit();
		}

		// The following calls should not be default-prevented
		IsHovered = false;
		ClearPressed(); // If we exit the frame we should not fire "FullPress" events.
	}

	internal bool PressIn(UiAction action)
	{
		SetPressed(action, pressed: true);

		var evt = new UiActionEvent(action);
		if (OnPressIn != null)
		{
			foreach (var handler in OnPressIn.GetInvocationList().OfType<EventHandler<Frame, UiActionEvent>>())
			{
				handler(this, evt);
				if (evt.Cancelled)
				{
					break;
				}
			}
		}

		if (evt.DefaultPrevented)
		{
			return evt.Handled;
		}

		// Do not inline! We want to call this even if "evt.Handled" is true
		var nativelyHandled = HandlePressIn(action);
		return evt.Handled || nativelyHandled;
	}

	internal bool PressOut(UiAction action)
	{
		var evt = new UiActionEvent(action);
		if (OnPressOut != null)
		{
			foreach (var handler in OnPressOut.GetInvocationList().OfType<EventHandler<Frame, UiActionEvent>>())
			{
				handler(this, evt);
				if (evt.Cancelled)
				{
					break;
				}
			}
		}

		if (evt.DefaultPrevented)
		{
			return evt.Handled;
		}

		var handled = HandlePressOut(action);
		if (!IsPressed(action))
		{
			return handled;
		}

		SetPressed(action, pressed: false);
		var fullPress = FullPress(action); // Do not inline! We want to call this even if "handled" is true
		return handled || fullPress;
	}

	private bool FullPress(UiAction action)
	{
		var evt = new UiActionEvent(action);
		if (OnFullPress != null)
		{
			foreach (var handler in OnFullPress.GetInvocationList().OfType<EventHandler<Frame, UiActionEvent>>())
			{
				handler(this, evt);
				if (evt.Cancelled)
				{
					break;
				}
			}
		}

		if (evt.DefaultPrevented)
		{
			return evt.Handled;
		}

		// Do not inline! We want to call this even if "evt.Handled" is true
		var nativelyHandled = HandleFullPress(action);
		return evt.Handled || nativelyHandled;
	}

	protected virtual bool HandlePressIn(UiAction action) => true;
	protected virtual bool HandlePressOut(UiAction action) => true;
	protected virtual bool HandleFullPress(UiAction action) => true;

	protected virtual void HandleCursorEnter()
	{
	}

	protected virtual void HandleCursorExit()
	{
	}

	public bool HitsThis(Vector2 position)
	{
		if (!Visible)
		{
			return false;
		}

		if (EnableInput)
		{
			return false;
		}

		return PositionIsInsideThisFrame(position);
	}

	public virtual Frame? Hit(Vector2 position)
	{
		if (!Visible)
		{
			return null;
		}

		if (!PositionIsInsideThisFrame(position))
		{
			return null;
		}

		var hitChild = PropagateInputsToChildren ? HitChildren(position) : null;
		var hitThis = EnableInput ? this : null;
		return hitChild ?? hitThis;
	}

	private Frame? HitChildren(Vector2 position)
	{
		for (var i = children.Count - 1; i >= 0; i--)
		{
			var child = children[i];
			var childHit = child.Hit(position);
			if (childHit != null)
			{
				return childHit;
			}
		}

		return null;
	}

	private bool PositionIsInsideThisFrame(Vector2 position)
	{
		var (pointX, pointY) = position;

		var bounds = TransformedBounds;
		var width = bounds.Width;
		var height = bounds.Height;
		var leftEdge = bounds.X;
		var rightEdge = leftEdge + width;
		var topEdge = bounds.Y;
		var bottomEdge = topEdge + height;

		var (topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius) = BorderRadii;

		// Check if the point is within the rectangular bounds
		if (pointX < leftEdge || pointX > rightEdge || pointY < topEdge || pointY > bottomEdge)
		{
			return false;
		}

		// Check if the point is within the rounded corners

		// Top-left corner
		if (pointX <= leftEdge + topLeftRadius && pointY <= topEdge + topLeftRadius)
		{
			var dx = pointX - (leftEdge + topLeftRadius);
			var dy = pointY - (topEdge + topLeftRadius);
			return dx * dx + dy * dy <= topLeftRadius * topLeftRadius;
		}

		// Top-right corner
		if (pointX >= rightEdge - topRightRadius && pointY <= topEdge + topRightRadius)
		{
			var dx = pointX - (rightEdge - topRightRadius);
			var dy = pointY - (topEdge + topRightRadius);
			return dx * dx + dy * dy <= topRightRadius * topRightRadius;
		}

		// Bottom-left corner
		if (pointX <= leftEdge + bottomLeftRadius && pointY >= bottomEdge - bottomLeftRadius)
		{
			var dx = pointX - (leftEdge + bottomLeftRadius);
			var dy = pointY - (bottomEdge - bottomLeftRadius);
			return dx * dx + dy * dy <= bottomLeftRadius * bottomLeftRadius;
		}

		// Bottom-right corner
		// ReSharper disable once InvertIf
		if (pointX >= rightEdge - bottomRightRadius && pointY >= bottomEdge - bottomRightRadius)
		{
			var dx = pointX - (rightEdge - bottomRightRadius);
			var dy = pointY - (bottomEdge - bottomRightRadius);
			return dx * dx + dy * dy <= bottomRightRadius * bottomRightRadius;
		}

		return true; // The point is inside the rounded rectangle
	}

	private enum ExplicitInputState
	{
		NotSet,
		Enabled,
		Disabled
	}
}