using MouseButtonEnum = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace CodeFirst.Input;

public class ButtonAxis : Axis
{
	private readonly Buttons negative;
	private readonly Buttons positive;

	private float value;

	public ButtonAxis(Buttons positive, Buttons negative)
	{
		this.positive = positive;
		this.negative = negative;
	}

	public override float Value => value;

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var mouseState = inputProvider.MouseState;
		var posPressed = mouseState.IsButtonDown(positive switch
		{
			Buttons.Left => MouseButtonEnum.Left,
			Buttons.Right => MouseButtonEnum.Right,
			Buttons.Middle => MouseButtonEnum.Middle,
			Buttons.ExtraOne => MouseButtonEnum.Button4,
			Buttons.ExtraTwo => MouseButtonEnum.Button5,
			_ => MouseButtonEnum.Left,
		});
		var negPressed = mouseState.IsButtonDown(negative switch
		{
			Buttons.Left => MouseButtonEnum.Left,
			Buttons.Right => MouseButtonEnum.Right,
			Buttons.Middle => MouseButtonEnum.Middle,
			Buttons.ExtraOne => MouseButtonEnum.Button4,
			Buttons.ExtraTwo => MouseButtonEnum.Button5,
			_ => MouseButtonEnum.Left,
		});

		value = 0;
		value += posPressed ? 1 : 0;
		value += negPressed ? -1 : 0;
	}
}