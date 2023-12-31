namespace CodeFirst.Input.Actions;

public record GamepadAxisInputAction(int Id, GamepadAxis Axis) : InputAction
{
	protected override float InternalValue { get; set; }

	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (e is not GamepadAxisInputEvent axisEvent)
		{
			return;
		}

		if (axisEvent.Axis != Axis || axisEvent.Id != Id)
		{
			return;
		}

		InternalValue = axisEvent.Value;
	}
}