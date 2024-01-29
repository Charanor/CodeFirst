using CodeFirst.Utils.Math;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input.Actions;

public abstract record InputAction
{
	private bool wasPressed;

	public bool IsPressed => InternalValue != 0;
	public bool JustPressed => IsPressed && !wasPressed;
	public bool JustReleased => !IsPressed && wasPressed;

	public float Value
	{
		get
		{
			var value = IsInverted ? -InternalValue : InternalValue;
			if (Deadzone <= 0)
			{
				return value;
			}

			var dz = MathF.Min(Deadzone, y: 1);
			var absValue = MathF.Abs(value);
			if (absValue < dz)
			{
				return 0;
			}

			var outputValue = DeadzoneMode switch
			{
				DeadzoneMode.Normalize => MathF.Sign(value) *
				                          MathHelper.MapRange(absValue, dz, valueMax: 1, resultMin: 0, resultMax: 1),
				DeadzoneMode.Clamp => value,
				_ => value // Fallback
			};

			return DeadzoneCurve.Apply(start: 0, end: 1, outputValue);
		}
	}

	protected abstract float InternalValue { get; set; }
	public float Deadzone { get; init; } = 0.08f;
	public DeadzoneMode DeadzoneMode { get; init; } = DeadzoneMode.Normalize;
	public Interpolation DeadzoneCurve { get; init; } = Interpolation.Linear;
	public bool IsInverted { get; init; }

	internal void Update()
	{
		wasPressed = IsPressed;
	}

	internal abstract void OnInput(InputManager manager, InputEvent e);

	public virtual MultiInputAction Add(InputAction action) => new(this, action);
	public virtual MultiInputAction Remove(InputAction action) => new(this);

	public static implicit operator InputAction(Keys key) => new KeyInputAction(key);

	public static implicit operator InputAction((Keys Negative, Keys Positive) axis) =>
		new KeyAxisInputAction(axis.Positive, axis.Negative);
	public static implicit operator InputAction(MouseButton button) => new MouseButtonInputAction(button);
	public static implicit operator InputAction(GamepadButton button) => new GamepadButtonInputAction(Id: 0, button);
	public static implicit operator InputAction(GamepadAxis axis) => new GamepadAxisInputAction(Id: 0, axis);

	public static InputAction operator !(InputAction action) => action.Inverted();
	public static InputAction operator -(InputAction action) => action.Inverted();

	public InputAction Inverted() => this with { IsInverted = true };

	public InputAction WithDeadzone(float deadzone, DeadzoneMode mode = DeadzoneMode.Normalize,
		Interpolation? deadzoneCurve = default) => this with
	{
		Deadzone = deadzone,
		DeadzoneMode = mode,
		DeadzoneCurve = deadzoneCurve ?? DeadzoneCurve
	};
}

public static class ActionExtensions
{
	public static InputAction AsAction(this Keys key) => key;
	public static InputAction AsAction(this MouseButton button) => button;
	public static InputAction AsAction(this GamepadButton button) => button;
	public static InputAction AsAction(this GamepadAxis axis) => axis;

	public static InputAction Inverted(this Keys key) => AsAction(key).Inverted();
	public static InputAction Inverted(this MouseButton button) => AsAction(button).Inverted();
	public static InputAction Inverted(this GamepadButton button) => AsAction(button).Inverted();
	public static InputAction Inverted(this GamepadAxis axis) => AsAction(axis).Inverted();

	public static InputAction WithDeadzone(this Keys key, float deadzone, DeadzoneMode mode = DeadzoneMode.Normalize,
		Interpolation? deadzoneCurve = default) => AsAction(key).WithDeadzone(deadzone, mode, deadzoneCurve);

	public static InputAction WithDeadzone(this MouseButton button, float deadzone,
		DeadzoneMode mode = DeadzoneMode.Normalize,
		Interpolation? deadzoneCurve = default) => AsAction(button).WithDeadzone(deadzone, mode, deadzoneCurve);

	public static InputAction WithDeadzone(this GamepadButton button, float deadzone,
		DeadzoneMode mode = DeadzoneMode.Normalize,
		Interpolation? deadzoneCurve = default) => AsAction(button).WithDeadzone(deadzone, mode, deadzoneCurve);

	public static InputAction WithDeadzone(this GamepadAxis axis, float deadzone,
		DeadzoneMode mode = DeadzoneMode.Normalize,
		Interpolation? deadzoneCurve = default) => AsAction(axis).WithDeadzone(deadzone, mode, deadzoneCurve);
}