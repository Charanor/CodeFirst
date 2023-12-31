using CodeFirst.Input.Actions;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using InputAction = CodeFirst.Input.Actions.InputAction;

namespace CodeFirst.Input;

public class NamedInputHandler
{
	private readonly Dictionary<string, InputAction> actions;

	public NamedInputHandler()
	{
		actions = new Dictionary<string, InputAction>();
	}

	public Vector2 MousePosition { get; private set; }

	public MultiInputAction this[string name]
	{
		get
		{
			var action = GetActionObject(name);
			if (action == null)
			{
				var multiInputAction = new MultiInputAction();
				actions.Add(name, multiInputAction);
				return multiInputAction;
			}

			if (action is MultiInputAction multi)
			{
				return multi;
			}

			multi = new MultiInputAction(action);
			actions[name] = multi;
			return multi;
		}
		set
		{
			if (!actions.ContainsKey(name))
			{
				actions.Add(name, value);
				return;
			}

			actions[name] = value;
		}
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

	public Vector2 Axis2D(string horizontal, string vertical, bool normalize = true)
	{
		var direction = new Vector2(Axis(horizontal), Axis(vertical));
		if (direction.LengthSquared == 0)
		{
			return Vector2.Zero;
		}
		
		return normalize ? direction.Normalized() : direction;
	}

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

	public InputAction? Define(string name, int id, GamepadButton button)
	{
		var action = new GamepadButtonInputAction(id, button);
		return actions.TryAdd(name, action) ? action : null;
	}

	public InputAction? Define(string name, int id, GamepadButton negative, GamepadButton positive)
	{
		var action = new GamepadButtonAxisInputAction(id, negative, positive);
		return actions.TryAdd(name, action) ? action : null;
	}

	public InputAction? Define(string name, int id, GamepadAxis axis)
	{
		var action = new GamepadAxisInputAction(id, axis);
		return actions.TryAdd(name, action) ? action : null;
	}

	public void Remove(string name)
	{
		actions.Remove(name);
	}
}