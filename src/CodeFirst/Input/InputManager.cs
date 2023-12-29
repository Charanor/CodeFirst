using CodeFirst.Utils.Events;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public class InputManager
{
	private readonly NativeWindow window;

	public InputManager(NativeWindow window)
	{
		this.window = window;

		window.MouseDown += GameOnMouse;
		window.MouseUp += GameOnMouse;
		window.MouseMove += GameOnMouseMove;
		window.MouseWheel += GameOnMouseWheel;

		window.KeyUp += GameOnKeyUp;
		window.KeyDown += GameOnKeyDown;
		window.TextInput += GameOnTextInput;
	}

	~InputManager()
	{
		window.MouseDown -= GameOnMouse;
		window.MouseUp -= GameOnMouse;
		window.MouseMove -= GameOnMouseMove;
		window.MouseWheel -= GameOnMouseWheel;

		window.KeyUp -= GameOnKeyUp;
		window.KeyDown -= GameOnKeyDown;
		window.TextInput -= GameOnTextInput;
	}

	private void GameOnTextInput(TextInputEventArgs e)
	{
		var evt = new TextInputEvent
		{
			Unicode = e.Unicode,
			Text = e.AsString
		};
		RaiseEvent(evt);
	}

	private void GameOnKeyDown(KeyboardKeyEventArgs e)
	{
		var evt = new KeyboardInputEvent
		{
			Key = e.Key,
			Modifiers = e.Modifiers,
			Action = e.IsRepeat 
				? OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat 
				: OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press
		};
		RaiseEvent(evt);
	}

	private void GameOnKeyUp(KeyboardKeyEventArgs e)
	{
		var evt = new KeyboardInputEvent
		{
			Key = e.Key,
			Modifiers = e.Modifiers,
			Action = e.IsRepeat 
				? OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat 
				: OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release
		};
		RaiseEvent(evt);
	}

	private void GameOnMouseWheel(MouseWheelEventArgs e)
	{
		var (x, y) = e.Offset;
		if (x != 0)
		{
			var evt = new MouseWheelInputEvent
			{
				Offset = x,
				Direction = ScrollDirection.Horizontal
			};
			RaiseEvent(evt);
		}

		if (y != 0)
		{
			var evt = new MouseWheelInputEvent
			{
				Offset = y,
				Direction = ScrollDirection.Vertical
			};
			RaiseEvent(evt);
		}
	}

	private void GameOnMouseMove(MouseMoveEventArgs e)
	{
		var evt = new MouseMoveInputEvent
		{
			Delta = e.Delta,
			Position = e.Position
		};
		RaiseEvent(evt);
	}

	private void GameOnMouse(MouseButtonEventArgs e)
	{
		var evt = new MouseButtonInputEvent
		{
			Button = e.Button,
			Modifiers = e.Modifiers,
			Action = e.Action
		};
		RaiseEvent(evt);
	}

	public event EventHandler<InputManager, InputEvent>? OnInput;

	private void RaiseEvent(InputEvent e)
	{
		if (OnInput == null)
		{
			return;
		}

		foreach (var handler in OnInput.GetInvocationList().OfType<EventHandler<InputManager, InputEvent>>())
		{
			handler(this, e);
			if (e.Handled)
			{
				break;
			}
		}
	}
}

public class KeyboardInputEvent : KeycodeInputEvent
{
	public Keys Key { get; init; }
}

public class MouseButtonInputEvent : KeycodeInputEvent
{
	public MouseButton Button { get; init; }
}

public class MouseMoveInputEvent : InputEvent
{
	public Vector2 Delta { get; init; }
	public Vector2 Position { get; init; }
}

public class MouseWheelInputEvent : InputEvent
{
	public float Offset { get; init; }
	public ScrollDirection Direction { get; init; }
}

public class TextInputEvent : InputEvent
{
	public string Text { get; init; } = "";
	public int Unicode { get; init; }
}

public abstract class KeycodeInputEvent : InputEvent
{
	public KeyModifiers Modifiers { get; init; }
	public OpenTK.Windowing.GraphicsLibraryFramework.InputAction Action { get; init; }

	public bool Alt => Modifiers.HasFlag(KeyModifiers.Alt);
	public bool Control => Modifiers.HasFlag(KeyModifiers.Control);
	public bool Shift => Modifiers.HasFlag(KeyModifiers.Shift);
	public bool Command => Modifiers.HasFlag(KeyModifiers.Super);
}

public abstract class InputEvent : EventArgs
{
	/// <summary>
	///     If this event has been handled by another event handler.
	/// </summary>
	public bool Handled { get; private set; }

	/// <summary>
	///     Marks this event as handled, preventing subsequent event listeners from being notified of the event.
	/// </summary>
	public void Handle() => Handled = true;
}