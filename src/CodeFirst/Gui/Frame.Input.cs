using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public partial class Frame
{
	/// <summary>
	///		If input events can hit this frame. Defaults to <c>false</c>.
	/// </summary>
	/// <remarks>
	///		If <c>false</c> input events will still propagate to this frame's children unless
	///		<see cref="PropagateInputsToChildren"/> is also <c>false</c>.
	/// </remarks>
	public bool EnableInput { get; set; } = false;

	/// <summary>
	///		If input events that hit this frame should propagate to its children. If <c>true</c> input events will
	///		propagate to this frame's children even if <see cref="EnableInput"/> if <c>false</c>. Defaults to
	///		<c>true</c>.
	/// </summary>
	public bool PropagateInputsToChildren { get; set; } = true;

	/// <summary>
	///     <c>true</c> if this frame is currently being hovered over. <c>false</c> otherwise.
	/// </summary>
	public bool IsHovered { get; private set; }

	public event EventHandler<Frame, UiEvent>? OnPressIn;
	public event EventHandler<Frame, UiEvent>? OnPressOut;
	public event EventHandler<Frame, UiEvent>? OnFullPress;

	public event Utils.Events.EventHandler<Frame>? OnCursorEnter;
	public event Utils.Events.EventHandler<Frame>? OnCursorExit;

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
		HandleCursorEnter();
		OnCursorEnter?.Invoke(this);
		IsHovered = true;
	}

	internal void CursorExit()
	{
		HandleCursorExit();
		OnCursorExit?.Invoke(this);
		IsHovered = false;

		// If we exit the frame we should not fire "FullPress" events.
		ClearPressed();
	}

	internal bool PressIn(UiAction action)
	{
		SetPressed(action, pressed: true);

		var evt = new UiEvent(action);
		OnPressIn?.Invoke(this, evt);

		var handled = evt.Handled;
		if (!evt.DefaultPrevented)
		{
			var nativelyHandled = HandlePressIn(action);
			handled = handled || nativelyHandled;
		}

		return handled;
	}

	internal bool PressOut(UiAction action)
	{
		var evt = new UiEvent(action);
		OnPressOut?.Invoke(this, evt);

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
		handled = handled || FullPress(action);
		return handled;
	}

	private bool FullPress(UiAction action)
	{
		var evt = new UiEvent(action);
		OnFullPress?.Invoke(this, evt);

		var handled = evt.Handled;
		if (!evt.DefaultPrevented)
		{
			var nativelyHandled = HandleFullPress(action);
			handled = handled || nativelyHandled;
		}

		return handled;
	}

	protected virtual bool HandlePressIn(UiAction action) => false;
	protected virtual bool HandlePressOut(UiAction action) => false;
	protected virtual bool HandleFullPress(UiAction action) => false;

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
}