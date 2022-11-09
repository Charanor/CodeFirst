namespace JXS.Input.Core;

using static OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

public record MouseButton : Axis
{
	private readonly Buttons buttons;
	private readonly ModifierKey modifier;

	public MouseButton(Buttons buttons, ModifierKey modifier = ModifierKey.None)
	{
		this.buttons = buttons;
		this.modifier = modifier;
	}

	public override float Value
	{
		get
		{
			var button = buttons switch
			{
				Buttons.Left => MouseState.IsButtonDown(Left),
				Buttons.Right => MouseState.IsButtonDown(Right),
				Buttons.Middle => MouseState.IsButtonDown(Middle),
				Buttons.ExtraOne => MouseState.IsButtonDown(Button4),
				Buttons.ExtraTwo => MouseState.IsButtonDown(Button5),
				_ => throw new InvalidOperationException("Cannot fetch state for button " + buttons)
			};
			return button && modifier.IsDown(KeyboardState) ? 1 : 0;
		}
	}
}