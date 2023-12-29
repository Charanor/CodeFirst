namespace CodeFirst.Input;

public class CopyNamedAxis : Axis
{
	private float value;

	public CopyNamedAxis(string axisName, InputSystem inputSystem)
	{
		AxisName = axisName;
		InputSystem = inputSystem;
	}

	public override float Value => value;
	public string AxisName { get; init; }
	public InputSystem InputSystem { get; init; }

	public override void Update(IInputProvider inputProvider, float gameTime)
	{
		base.Update(inputProvider, gameTime);
		value = InputSystem.Axis(AxisName);
	}

	public override bool HasSameBindings(Axis other) => other is CopyNamedAxis axis && axis.AxisName == AxisName;

	public void Deconstruct(out string axisName, out InputSystem inputSystem)
	{
		axisName = AxisName;
		inputSystem = InputSystem;
	}
}