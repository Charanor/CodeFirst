namespace CodeFirst.Input.Actions;

public record GamepadButtonInputAction(int Id, GamepadButton Button) : InputAction
{
	protected override float InternalValue {get;set;}

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (e is not GamepadButtonInputEvent btn)
		{
			return;
		}

		if (btn.Button != Button || btn.Id != Id)
		{
			return;
		}

		InternalValue = btn.Action switch
		{
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Press => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Repeat => 1,
			OpenTK.Windowing.GraphicsLibraryFramework.InputAction.Release => 0,
			_ => 0 // Fallback
		};
	}
}