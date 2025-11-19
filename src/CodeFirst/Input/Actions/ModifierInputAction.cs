using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input.Actions;

public record ModifierInputAction(KeyModifiers Modifier) : InputAction
{
	protected override float InternalValue { get; set; }
	
	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (e is not KeyboardInputEvent ke)
		{
			return;
		}

		if ((ke.Modifiers & Modifier) != Modifier)
		{
			InternalValue = 0;
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