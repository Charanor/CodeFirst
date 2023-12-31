using System.Collections;
using System.Diagnostics.CodeAnalysis;
using CodeFirst.Input;
using Facebook.Yoga;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using InputAction = OpenTK.Windowing.GraphicsLibraryFramework.InputAction;

namespace CodeFirst.Gui;

public class Scene : IEnumerable<Frame>
{
	private readonly List<Frame> frames;
	private Vector2 mousePosition;

	private Frame? previousHoverFrame;

	public Scene(IGraphicsProvider graphicsProvider)
	{
		GraphicsProvider = graphicsProvider;
		frames = new List<Frame>();
	}

	public IGraphicsProvider GraphicsProvider { get; init; }

	public Vector2 Size { get; set; } = new(float.NaN, float.NaN);

	public Vector2 MousePosition
	{
		get => mousePosition;
		set
		{
			mousePosition = value;
			var hit = Hit(value);
			if (hit == previousHoverFrame)
			{
				return;
			}

			previousHoverFrame?.CursorExit();
			previousHoverFrame = hit;
			hit?.CursorEnter();
		}
	}

	public IEnumerator<Frame> GetEnumerator() => frames.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void HandleInput(InputManager manager, InputEvent e)
	{
		switch (e)
		{
			case MouseButtonInputEvent mouseButtonEvent:
			{
				switch (mouseButtonEvent.Button)
				{
					case MouseButton.Left:
						if (mouseButtonEvent.Action == InputAction.Press)
						{
							if (PressIn(UiAction.Primary))
							{
								e.Handle();
							}
						}
						else if (mouseButtonEvent.Action == InputAction.Release)
						{
							// NOTE: Do not "handle" this event, it needs to be propagated.
							PressOut(UiAction.Primary);
						}

						break;
					case MouseButton.Right:
						if (mouseButtonEvent.Action == InputAction.Press)
						{
							if (PressIn(UiAction.Secondary))
							{
								e.Handle();
							}
						}
						else if (mouseButtonEvent.Action == InputAction.Release)
						{
							// NOTE: Do not "handle" this event, it needs to be propagated.
							PressOut(UiAction.Secondary);
						}
						break;
				}

				break;
			}
			case MouseWheelInputEvent mouseWheelEvent:
				if (ScrollBy(mouseWheelEvent.Offset * 100))
				{
					e.Handle();
				}
				break;
			case MouseMoveInputEvent mouseMoveEvent:
				// NOTE Do not mark this event as handled
				mousePosition = mouseMoveEvent.Position;
				break;
		}
	}

	public IReadOnlyList<Frame> GetFrames() => frames;

	public void Update(float delta)
	{
		foreach (var frame in frames.Where(c => c.Visible))
		{
			frame.Update(delta);
		}
	}

	public void Draw()
	{
		GraphicsProvider.Begin();
		foreach (var frame in frames.Where(c => c.Visible))
		{
			frame.ApplyStyle();
			frame.CalculateLayout(Size.X, Size.Y);
			frame.Draw(GraphicsProvider);
		}

		GraphicsProvider.End();
	}

	public void AddFrame<TFrame>(TFrame frame) where TFrame : Frame
	{
		frame.Scene = this;
		frames.Add(frame);
	}

	public bool RemoveFrame<TFrame>(TFrame frame) where TFrame : Frame
	{
		var removed = frames.Remove(frame);
		if (removed)
		{
			frame.Scene = null;
		}

		return removed;
	}

	/// <summary>
	///     Gets a component in this scene with the given "id".
	/// </summary>
	/// <param name="id">the id of the component</param>
	/// <typeparam name="TFrame">the type of the component</typeparam>
	/// <returns>the found component with given id of type T</returns>
	/// <exception cref="NullReferenceException">if no component with given id exists</exception>
	/// <exception cref="InvalidOperationException">if a component with given id exists, but is not of type T</exception>
	public TFrame GetFrame<TFrame>(string id) where TFrame : Frame
	{
		foreach (var frame in frames)
		{
			if (frame.Id == id)
			{
				if (frame is TFrame cmp)
				{
					return cmp;
				}

				throw new InvalidOperationException(
					$"A component with id {id} exists, but is of type {frame.GetType().Name}, expected {typeof(TFrame).Name}.");
			}

			var child = frame.GetChild<TFrame>(id);
			if (child != null)
			{
				return child;
			}
		}

		throw new NullReferenceException($"No component with id {id} exists.");
	}

	public bool TryGetFrame<TFrame>(string id, [NotNullWhen(true)] out TFrame? outFrame) where TFrame : Frame
	{
		foreach (var frame in frames)
		{
			if (frame.Id == id)
			{
				if (frame is TFrame cmp)
				{
					outFrame = cmp;
					return true;
				}

				outFrame = null;
				return false;
			}

			var child = frame.GetChild<TFrame>(id);
			if (child == null)
			{
				continue;
			}

			outFrame = child;
			return true;
		}

		outFrame = null;
		return false;
	}

	public Frame? Hit(Vector2 position)
	{
		// Reverse iteration
		for (var i = frames.Count - 1; i >= 0; i--)
		{
			var component = frames[i].Hit(position);
			if (component != null)
			{
				return component;
			}
		}

		return null;
	}

	public bool PressIn(UiAction action)
	{
		var hit = Hit(MousePosition);
		if (hit == null)
		{
			return false;
		}

		return hit.PressIn(action);
	}

	public bool PressOut(UiAction action)
	{
		var hit = Hit(MousePosition);
		if (hit == null)
		{
			return false;
		}

		return hit.PressOut(action);
	}

	public bool ScrollBy(Vector2 scroll) => ScrollBy(scroll.X, scroll.Y);
	public bool ScrollBy(float vertical) => ScrollBy(horizontal: 0, vertical);

	public bool ScrollBy(float horizontal, float vertical)
	{
		if (horizontal == 0 && vertical == 0)
		{
			return false;
		}

		var hit = Hit(MousePosition);
		var hasHitSomething = hit != null;
		while (hit != null && hit.Overflow != YogaOverflow.Scroll)
		{
			hit = hit.Parent;
		}

		if (hit == null)
		{
			// This is because we still want to prevent the scroll event from propagating outside of the scene even if
			// we didn't actually scroll any element.
			return hasHitSomething;
		}

		hit.ScrollBy(horizontal, vertical);
		return true;
	}
}