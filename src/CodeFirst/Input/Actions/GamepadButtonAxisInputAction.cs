namespace CodeFirst.Input.Actions;

public record GamepadButtonAxisInputAction(int Id, GamepadButton Negative, GamepadButton Positive) : InputAction
{
	private float positiveValue;
	private float negativeValue;

	protected override float InternalValue { get; set; }

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		HandlePositiveInput(e);
		HandleNegativeInput(e);
		InternalValue = positiveValue - negativeValue;
	}

	private void HandlePositiveInput(InputEvent e)
	{
		if (e is not GamepadButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Positive || btn.Id != Id)
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
		if (e is not GamepadButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Negative || btn.Id != Id)
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