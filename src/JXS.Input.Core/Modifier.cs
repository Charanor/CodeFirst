namespace JXS.Input.Core;

public record Modifier : Axis
{
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

	public Modifier(Axis axis, string axisName)
	{
		Axis = axis;
		ModifierAxis = new CopyNamedAxis(axisName);
	}


	public override float Value
	{
		get
		{
			var modifierAxisPressed = ModifierAxis?.Pressed ?? true;
			var modifierKeyDown = ModifierKey?.IsDown(KeyboardState) ?? true;
			if (modifierAxisPressed && modifierKeyDown)
			{
				return Axis.Value;
			}

			return 0;
		}
	}

	public Axis Axis { get; }
	public Axis? ModifierAxis { get; }
	public ModifierKey? ModifierKey { get; }

	public override void Update(float delta)
	{
		base.Update(delta);
		Axis.Update(delta);
		ModifierAxis?.Update(delta);
	}
}