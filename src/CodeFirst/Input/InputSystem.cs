// using SDL2;

using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public class InputSystem
{
	private readonly IDictionary<string, MultiAxis> axes;

	public InputSystem(IInputProvider inputProvider)
	{
		InputProvider = inputProvider;
		axes = new Dictionary<string, MultiAxis>();
	}

	public IInputProvider InputProvider { get; }

	public bool Enabled { get; set; } = true;

	public Vector2 MousePosition { get; private set; }

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

	public void Update(float delta)
	{
		foreach (var axis in axes.Values)
		{
			axis.Consumed = false;
			axis.Update(InputProvider, delta);
		}

		MousePosition = InputProvider.MouseState.Position;
	}

	public Axis? GetAxisObject(string name)
	{
		if (!Enabled)
		{
			return null;
		}

		if (!axes.TryGetValue(name, out var axis))
		{
			return null;
		}

		return axis.Consumed ? null : axis;
	}

	public float Axis(string name) => GetAxisObject(name)?.Value ?? 0;

	public Vector2 Axis2D(string horizontal, string vertical, bool normalize = true)
	{
		var direction = new Vector2(Axis(horizontal), Axis(vertical));
		if (normalize && direction.LengthSquared > 0)
		{
			direction.Normalize();
		}

		return direction;
	}

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

	/// <summary>
	///     Consumes the given input, causing it to return <c>0</c> or <c>false</c> etc. when queried. Lasts until the
	///     next call to <see cref="Update" />.
	/// </summary>
	/// <param name="name">the name of the input</param>
	/// <seealso cref="ConsumeInputsWithSharedBindings" />
	public void ConsumeInput(string name)
	{
		if (!axes.TryGetValue(name, out var axis))
		{
			return;
		}

		axis.Consumed = true;
	}

	/// <summary>
	///     Consumes all inputs that have the exact same keyboard binding as the given input. Lasts until the next call
	///		to <see cref="Update"/>.
	/// </summary>
	/// <param name="name"></param>
	/// <seealso cref="ConsumeInput" />
	public void ConsumeInputsWithSharedBindings(string name)
	{
		var referenceAxis = GetAxisObject(name);
		if (referenceAxis == null)
		{
			return;
		}
		
		foreach (var (_, axis) in axes)
		{
			if (axis.HasSameBindings(referenceAxis))
			{
				axis.Consumed = true;
			}
		}
	}
}