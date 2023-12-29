using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public class NamedInputHandler
{
	private readonly Dictionary<string, InputAction> actions;

	public NamedInputHandler()
	{
		actions = new Dictionary<string, InputAction>();
	}
	
	public void HandleInput(InputManager sender, InputEvent e)
	{
		if (e is MouseMoveInputEvent mouseMove)
		{
			MousePosition = mouseMove.Position;
		}
		
		foreach (var (_, action) in actions)
		{
			action.OnInput(sender, e);
		}
	}
	
	public Vector2 MousePosition { get; private set; }

	public void Update()
	{
		foreach (var (_, action) in actions)
		{
			action.Update();
		}
	}

	public bool JustPressed(string name) => actions.TryGetValue(name, out var action) && action.JustPressed;
	public bool JustReleased(string name) => actions.TryGetValue(name, out var action) && action.JustReleased;
	public bool IsPressed(string name) => actions.TryGetValue(name, out var action) && action.IsPressed;
	public float Axis(string name) => actions.TryGetValue(name, out var action) ? action.Value : 0;

	public Vector2 Axis2D(string horizontal, string vertical) => new(
		Axis(horizontal),
		Axis(vertical)
	);

	public InputAction? GetActionObject(string name) => actions.TryGetValue(name, out var action) ? action : null;

	public InputAction? Define(string name, Keys key, KeyModifiers modifiers = default)
	{
		var action = new KeyInputAction(key, modifiers);
		return actions.TryAdd(name, action) ? action : null;
	}

	public InputAction? Define(string name, Keys negative, Keys positive, KeyModifiers modifiers = default)
	{
		var action = new KeyAxisInputAction(positive, negative, modifiers);
		return actions.TryAdd(name, action) ? action : null;
	}

	public InputAction? Define(string name, MouseButton mouseButton, KeyModifiers modifiers = default)
	{
		var action = new MouseButtonInputAction(mouseButton, modifiers);
		return actions.TryAdd(name, action) ? action : null;
	}

	public InputAction? Define(string name, MouseButton negative, MouseButton positive,
		KeyModifiers modifiers = default)
	{
		var action = new MouseAxisInputAction(positive, negative, modifiers);
		return actions.TryAdd(name, action) ? action : null;
	}

	public void Remove(string name)
	{
		actions.Remove(name);
	}
}

public abstract class InputAction
{
	private bool wasPressed;

	public bool IsPressed => Value != 0;
	public bool JustPressed => IsPressed && !wasPressed;
	public bool JustReleased => !IsPressed && wasPressed;

	public abstract float Value { get; }

	internal void Update()
	{
		wasPressed = IsPressed;
	}

	internal abstract void OnInput(InputManager manager, InputEvent e);
}

file class KeyInputAction : InputAction
{
	private float value;

	public KeyInputAction(Keys key, KeyModifiers modifiers)
	{
		Key = key;
		Modifiers = modifiers;
	}

	public Keys Key { get; }
	public KeyModifiers Modifiers { get; }

	public override float Value => value;

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (e is not KeyboardInputEvent ke)
		{
			return;
		}

		if (ke.Key != Key || (ke.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		value = ke.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}

file class KeyAxisInputAction : InputAction
{
	private float positiveValue;
	private float negativeValue;

	public KeyAxisInputAction(Keys positive, Keys negative, KeyModifiers modifiers = default)
	{
		Positive = positive;
		Negative = negative;
		Modifiers = modifiers;
	}

	public Keys Positive { get; }
	public Keys Negative { get; }
	public KeyModifiers Modifiers { get; }

	public override float Value => positiveValue - negativeValue;

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		HandlePositiveInput(e);
		HandleNegativeInput(e);
	}

	private void HandlePositiveInput(InputEvent e)
	{
		if (e is not KeyboardInputEvent ke)
		{
			return;
		}

		if (ke.Key != Positive || (ke.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		positiveValue = ke.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}

	private void HandleNegativeInput(InputEvent e)
	{
		if (e is not KeyboardInputEvent ke)
		{
			return;
		}

		if (ke.Key != Negative || (ke.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		negativeValue = ke.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}

file class MouseButtonInputAction : InputAction
{
	private float value;

	public MouseButtonInputAction(MouseButton button, KeyModifiers modifiers)
	{
		Button = button;
		Modifiers = modifiers;
	}

	public MouseButton Button { get; }
	public KeyModifiers Modifiers { get; }

	public override float Value => value;

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (e is not MouseButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Button || (btn.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		value = btn.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}

file class MouseAxisInputAction : InputAction
{
	private float positiveValue;
	private float negativeValue;

	public MouseAxisInputAction(MouseButton positive, MouseButton negative, KeyModifiers modifiers = default)
	{
		Positive = positive;
		Negative = negative;
		Modifiers = modifiers;
	}

	public MouseButton Positive { get; }
	public MouseButton Negative { get; }
	public KeyModifiers Modifiers { get; }

	public override float Value => positiveValue - negativeValue;

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		HandlePositiveInput(e);
		HandleNegativeInput(e);
	}

	private void HandlePositiveInput(InputEvent e)
	{
		if (e is not MouseButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Positive || (btn.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		positiveValue = btn.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}

	private void HandleNegativeInput(InputEvent e)
	{
		if (e is not MouseButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Negative || (btn.Modifiers & Modifiers) != Modifiers)
		{
			return;
		}

		negativeValue = btn.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}