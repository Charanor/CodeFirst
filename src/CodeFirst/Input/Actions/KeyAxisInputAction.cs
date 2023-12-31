using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input.Actions;

public record KeyAxisInputAction(Keys Positive, Keys Negative, KeyModifiers Modifiers = default) : InputAction
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