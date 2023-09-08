using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public class KeyboardAxis : Axis
{
	private readonly Keys negative;
	private readonly Keys positive;

	private float value;

	public KeyboardAxis(Keys positive, Keys negative)
	{
		this.positive = positive;
		this.negative = negative;
	}

	public override float Value => value;

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var keyboardState = inputProvider.KeyboardState;
		var posPressed = keyboardState.IsKeyDown(positive);
		var negPressed = keyboardState.IsKeyDown(negative);

		value = 0;
		value += posPressed ? 1 : 0;
		value += negPressed ? -1 : 0;
	}
}