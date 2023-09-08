namespace CodeFirst.Input;

public class ModifierAxis : Axis
{
	private float value;

	public ModifierAxis(ModifierKey key)
	{
		Key = key;
	}

	public override float Value => value;
	public ModifierKey Key { get; init; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		value = Key.IsDown(inputProvider.KeyboardState) ? 0 : 1;
	}

	public void Deconstruct(out ModifierKey key)
	{
		key = Key;
	}
}