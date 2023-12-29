namespace CodeFirst.Input;

public class Modifier : Axis
{
	private float value;

	public Modifier(Axis axis, Axis modifierAxis)
	{
		Axis = axis;
		ModifierAxis = modifierAxis;
	}

	public Modifier(Axis axis, ModifierKey modifierKey)
	{
		Axis = axis;
		ModifierKey = modifierKey;
	}

	public Modifier(Axis axis, string axisName, InputSystem inputSystem)
	{
		Axis = axis;
		ModifierAxis = new CopyNamedAxis(axisName, inputSystem);
	}

	public override float Value => value;

	public Axis Axis { get; }
	public Axis? ModifierAxis { get; }
	public ModifierKey? ModifierKey { get; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		Axis.Update(inputProvider, delta);
		ModifierAxis?.Update(inputProvider, delta);
		var modifierAxisPressed = ModifierAxis?.Pressed ?? true;
		var modifierKeyDown = ModifierKey?.IsDown(inputProvider.KeyboardState) ?? true;
		value = modifierAxisPressed && modifierKeyDown ? Axis.Value : 0;
	}

	public override bool HasSameBindings(Axis other) => other is Modifier axis && 
	                                                    axis.Axis == Axis &&
	                                                    axis.ModifierAxis == ModifierAxis &&
	                                                    axis.ModifierKey == ModifierKey;
}