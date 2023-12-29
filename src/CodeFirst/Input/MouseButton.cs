namespace CodeFirst.Input;

using static OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

public class MouseButton : Axis
{
	private float value;

	public MouseButton(Buttons button, ModifierKey modifier = ModifierKey.None)
	{
		Button = button;
		Modifier = modifier;
	}

	public override float Value => value;
	public Buttons Button { get; init; }
	public ModifierKey Modifier { get; init; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var button = Button switch
		{
			Buttons.Left => inputProvider.MouseState.IsButtonDown(Left),
			Buttons.Right => inputProvider.MouseState.IsButtonDown(Right),
			Buttons.Middle => inputProvider.MouseState.IsButtonDown(Middle),
			Buttons.ExtraOne => inputProvider.MouseState.IsButtonDown(Button4),
			Buttons.ExtraTwo => inputProvider.MouseState.IsButtonDown(Button5),
			_ => throw new InvalidOperationException("Cannot fetch state for button " + Button)
		};
		value = button && Modifier.IsDown(inputProvider.KeyboardState) ? 1 : 0;
	}

	public override bool HasSameBindings(Axis other) =>
		other is MouseButton axis && axis.Button == Button && axis.Modifier == Modifier;

	public void Deconstruct(out Buttons button, out ModifierKey modifier)
	{
		button = Button;
		modifier = Modifier;
	}
}