using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public class KeyboardButton : Axis
{
	private readonly Keys key;
	private readonly ModifierKey modifier;

	private float value;

	public KeyboardButton(Keys key, ModifierKey modifier = ModifierKey.None)
	{
		this.key = key;
		this.modifier = modifier;
	}

	public override float Value => value;

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var keyboardState = inputProvider.KeyboardState;
		value = keyboardState.IsKeyDown(key) && modifier.IsDown(keyboardState) ? 1 : 0;
	}

	public override bool HasSameBindings(Axis other) =>
		other is KeyboardButton axis && axis.key == key && axis.modifier == modifier;
}