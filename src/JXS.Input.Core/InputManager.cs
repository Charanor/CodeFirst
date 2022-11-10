// using SDL2;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public class InputManager
{
	public static readonly InputManager Instance = new();

	private readonly IDictionary<string, MultiAxis> axes;

	private InputManager()
	{
		axes = new Dictionary<string, MultiAxis>();
	}

	internal KeyboardState? KeyboardState { get; private set; }
	internal MouseState? MouseState { get; private set; }

	public MultiAxis this[string name]
	{
		get
		{
			if (!axes.ContainsKey(name))
			{
				axes[name] = new MultiAxis();
			}

			return axes[name];
		}

		set => axes[name] = value;
	}

	public void Update(float delta, MouseState mouseState, KeyboardState keyboardState)
	{
		MouseState = mouseState;
		KeyboardState = keyboardState;
		foreach (var axis in axes.Values)
		{
			axis.Update(delta);
		}
	}

	public Vector2 GetMousePos() => new(MouseState?.X ?? 0, MouseState?.Y ?? 0);

	public Axis? GetAxisObject(string name) => axes.TryGetValue(name, out var axis) ? axis : null;

	public float Axis(string name) => GetAxisObject(name)?.Value ?? 0;

	public bool Button(string buttonName) => GetAxisObject(buttonName)?.Pressed ?? false;

	public bool JustPressed(string buttonName) => GetAxisObject(buttonName)?.JustPressed ?? false;

	public bool JustReleased(string buttonName) => GetAxisObject(buttonName)?.JustReleased ?? false;

	public void Define(string name, Axis axis) => this[name] += axis;

	public void Define(string name, ModifierKey modifierKey) =>
		this[name] += new ModifierAxis(modifierKey);

	public void Define(string name, Keys keyboardButton, ModifierKey modifierKey = ModifierKey.None) =>
		this[name] += new KeyboardButton(keyboardButton, modifierKey);

	public void Define(string name, Buttons mouseButton, ModifierKey modifierKey = ModifierKey.None) =>
		this[name] += new MouseButton(mouseButton, modifierKey);

	public void Define(string name, ScrollDirection direction) => this[name] += new ScrollWheel(direction);

	public void Define(string name, MouseDirection direction) => this[name] += new MouseMovement(direction);

	public void Define(string name, Keys positive, Keys negative) => this[name] += new KeyboardAxis(positive, negative);
}