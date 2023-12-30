using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input.Actions;

public record KeyInputAction(Keys Key, KeyModifiers Modifiers = default) : InputAction
{
	protected override float InternalValue { get; set; }

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

		InternalValue = ke.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}