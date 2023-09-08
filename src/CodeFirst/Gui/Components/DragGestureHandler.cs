using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Gui.Components;

public class DragGestureHandler : GestureHandler
{
	private bool isHovering;
	private bool isDragging;
	private Vector2 startPosition;
	private Vector2 lastPosition;

	public DragGestureHandler()
	{
		OnEnter += (_, _) => isHovering = true;
		OnExit += (_, _) => isHovering = false;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (!Visible)
		{
			isDragging = false;
			return;
		}

		// If we are dragging, we don't care if we left the area of the gesture handler and are no longer hovering it
		if (!isHovering && !isDragging)
		{
			isDragging = false;
			return;
		}

		if (InputProvider == null)
		{
			isDragging = false;
			return;
		}

		if (!InputProvider.IsPressed(GuiInputAction.Primary))
		{
			isDragging = false;
			return;
		}

		if (!isDragging)
		{
			// We just started dragging, reset
			startPosition = InputProvider.MousePosition;
			lastPosition = startPosition;
			isDragging = true;
		}

		// We are dragging
		var currentPosition = InputProvider.MousePosition;
		var mouseDelta = currentPosition - lastPosition;
		lastPosition = currentPosition;

		OnDrag?.Invoke(this, new DragEvent(startPosition, currentPosition, mouseDelta));
	}

	public event EventHandler<DragGestureHandler, DragEvent>? OnDrag;
}

public record DragEvent(Vector2 StartPosition, Vector2 CurrentPosition, Vector2 ChangeSinceLastEvent);