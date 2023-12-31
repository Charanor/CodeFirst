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

	public bool LatestInputWasGamepad { get; private set; }

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

	public void Update()
	{
		foreach (var gamepad in window.JoystickStates)
		{
			if (gamepad == null)
			{
				// Non-connected gamepads are "null"
				continue;
			}

			foreach (int gamepadButton in Enum.GetValues<GamepadButton>())
			{
				if (gamepadButton >= gamepad.ButtonCount)
				{
					// Not all gamepads have all the buttons
					break;
				}

				if (gamepad.IsButtonPressed(gamepadButton))
				{
					LatestInputWasGamepad = true;
					var evt = new GamepadButtonInputEvent
					{
						Id = gamepad.Id,
						Name = gamepad.Name,
						Action = InputAction.Press,
						Button = (GamepadButton)gamepadButton
					};
					RaiseEvent(evt);
				}
				else if (gamepad.IsButtonReleased(gamepadButton))
				{
					LatestInputWasGamepad = true;
					var evt = new GamepadButtonInputEvent
					{
						Id = gamepad.Id,
						Name = gamepad.Name,
						Action = InputAction.Release,
						Button = (GamepadButton)gamepadButton
					};
					RaiseEvent(evt);
				}
				else if (gamepad.IsButtonDown(gamepadButton))
				{
					LatestInputWasGamepad = true;
					var evt = new GamepadButtonInputEvent
					{
						Id = gamepad.Id,
						Name = gamepad.Name,
						Action = InputAction.Repeat,
						Button = (GamepadButton)gamepadButton
					};
					RaiseEvent(evt);
				}
			}

			foreach (int gamepadAxis in Enum.GetValues<GamepadAxis>())
			{
				if (gamepadAxis >= gamepad.AxisCount)
				{
					// Not all gamepads have all the axes
					break;
				}

				var currentAxis = gamepad.GetAxis(gamepadAxis);
				var previousAxis = gamepad.GetAxisPrevious(gamepadAxis);

				if (Math.Abs(currentAxis - previousAxis) < 0.0001f)
				{
					continue;
				}

				LatestInputWasGamepad = true;
				var evt = new GamepadAxisInputEvent
				{
					Id = gamepad.Id,
					Name = gamepad.Name,
					Axis = (GamepadAxis)gamepadAxis,
					Value = currentAxis,
					Delta = currentAxis - previousAxis
				};
				RaiseEvent(evt);
			}
		}
	}

	public void UpdateGamepadMappings(string mapping)
	{
		GLFW.UpdateGamepadMappings(mapping);
	}

	private void GameOnTextInput(TextInputEventArgs e)
	{
		// NOTE: Don't set "LatestInputWasGamepad" here.
		var evt = new TextInputEvent
		{
			Unicode = e.Unicode,
			Text = e.AsString
		};
		RaiseEvent(evt);
	}

	private void GameOnKeyDown(KeyboardKeyEventArgs e)
	{
		LatestInputWasGamepad = false;
		var evt = new KeyboardInputEvent
		{
			Key = e.Key,
			Modifiers = e.Modifiers,
			Action = e.IsRepeat
				? InputAction.Repeat
				: InputAction.Press
		};
		RaiseEvent(evt);
	}

	private void GameOnKeyUp(KeyboardKeyEventArgs e)
	{
		LatestInputWasGamepad = false;
		var evt = new KeyboardInputEvent
		{
			Key = e.Key,
			Modifiers = e.Modifiers,
			Action = e.IsRepeat
				? InputAction.Repeat
				: InputAction.Release
		};
		RaiseEvent(evt);
	}

	private void GameOnMouseWheel(MouseWheelEventArgs e)
	{
		LatestInputWasGamepad = false;
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
		// NOTE: Maybe we should not set "LatestInputWasGamepad" here, because on some operative systems gamepad input
		// might move the hardware mouse, causing this event to trigger. For now let's keep it.
		LatestInputWasGamepad = false;
		var evt = new MouseMoveInputEvent
		{
			Delta = e.Delta,
			Position = e.Position
		};
		RaiseEvent(evt);
	}

	private void GameOnMouse(MouseButtonEventArgs e)
	{
		LatestInputWasGamepad = false;
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
	public required Keys Key { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Key)}: {Key}";
}

public class MouseButtonInputEvent : KeycodeInputEvent
{
	public required MouseButton Button { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Button)}: {Button}";
}

public class MouseMoveInputEvent : InputEvent
{
	public required Vector2 Delta { get; init; }
	public required Vector2 Position { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Delta)}: {Delta}, {nameof(Position)}: {Position}";
}

public class MouseWheelInputEvent : InputEvent
{
	public required float Offset { get; init; }
	public required ScrollDirection Direction { get; init; }

	public override string ToString() =>
		$"{base.ToString()}, {nameof(Offset)}: {Offset}, {nameof(Direction)}: {Direction}";
}

public class TextInputEvent : InputEvent
{
	public required string Text { get; init; } = "";
	public required int Unicode { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Text)}: {Text}, {nameof(Unicode)}: {Unicode}";
}

public abstract class KeycodeInputEvent : InputEvent
{
	public required KeyModifiers Modifiers { get; init; }
	public required InputAction Action { get; init; }

	public bool Alt => Modifiers.HasFlag(KeyModifiers.Alt);
	public bool Control => Modifiers.HasFlag(KeyModifiers.Control);
	public bool Shift => Modifiers.HasFlag(KeyModifiers.Shift);
	public bool Command => Modifiers.HasFlag(KeyModifiers.Super);

	public override string ToString() =>
		$"{base.ToString()}, {nameof(Modifiers)}: {Modifiers}, {nameof(Action)}: {Action}, {nameof(Alt)}: {Alt}, {nameof(Control)}: {Control}, {nameof(Shift)}: {Shift}, {nameof(Command)}: {Command}";
}

public class GamepadButtonInputEvent : GamepadInputEvent
{
	public required GamepadButton Button { get; init; }
	public required InputAction Action { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Button)}: {Button}, {nameof(Action)}: {Action}";
}

public class GamepadAxisInputEvent : GamepadInputEvent
{
	public required GamepadAxis Axis { get; init; }

	/// <summary>
	///     The current value of the axis.
	/// </summary>
	public required float Value { get; init; }

	/// <summary>
	///     The change of axis value since the previous <see cref="GamepadAxisInputEvent" /> for this axis.
	/// </summary>
	public required float Delta { get; init; }

	public override string ToString() =>
		$"{base.ToString()}, {nameof(Axis)}: {Axis}, {nameof(Value)}: {Value}, {nameof(Delta)}: {Delta}";
}

public abstract class GamepadInputEvent : InputEvent
{
	/// <summary>
	///     The id of the gamepad that triggered this event
	/// </summary>
	public required int Id { get; init; }

	/// <summary>
	///     The human-readable name of the gamepad that triggered this event.
	/// </summary>
	public required string Name { get; init; }

	public override string ToString() => $"{base.ToString()}, {nameof(Id)}: {Id}, {nameof(Name)}: {Name}";
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

	public override string ToString() => $"Event: {GetType().Name} {nameof(Handled)}: {Handled}";
}