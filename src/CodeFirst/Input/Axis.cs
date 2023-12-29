using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public abstract class Axis
{
	private bool wasPressed;

	public abstract float Value { get; }

	public bool Consumed { get; internal set; }

	public bool Pressed => Value != 0;
	public bool JustPressed { get; private set; }
	public bool JustReleased { get; private set; }

	public virtual void Update(IInputProvider inputProvider, float delta)
	{
		JustPressed = !wasPressed && Pressed;
		JustReleased = wasPressed && !Pressed;
		wasPressed = Pressed;
	}

	public static Axis Create(Keys keyboardButton)
		=> new KeyboardButton(keyboardButton);

	public static Axis Create(Buttons mouseButton)
		=> new MouseButton(mouseButton);

	public static Axis Create(Keys positive, Keys negative)
		=> new KeyboardAxis(positive, negative);

	public static Axis Create(ScrollDirection direction)
		=> new ScrollWheel(direction);

	public static Axis Create(MouseDirection direction)
		=> new MouseMovement(direction);

	public static Axis Copy(string axisName, InputSystem inputSystem)
		=> new CopyNamedAxis(axisName, inputSystem);

	public abstract bool HasSameBindings(Axis other);
}