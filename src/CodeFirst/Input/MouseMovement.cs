using OpenTK.Mathematics;

namespace CodeFirst.Input;

public class MouseMovement : Axis
{
	private Vector2 prevValue = new(float.NaN, float.NaN);
	private Vector2 value = new(float.NaN, float.NaN);

	public MouseMovement(MouseDirection axis)
	{
		Axis = axis;
	}

	public override float Value => Axis switch
	{
		MouseDirection.Vertical => value.Y,
		MouseDirection.Horizontal => value.X,
		MouseDirection.Both => value.Length,
		_ => throw new ArgumentOutOfRangeException()
	};

	public MouseDirection Axis { get; init; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var currentValue = GetMousePosition(inputProvider);
		var difference = float.IsNaN(prevValue.X) || float.IsNaN(prevValue.Y) ? Vector2.Zero : currentValue - prevValue;
		prevValue = currentValue;
		value = difference;
	}

	private Vector2 GetMousePosition(IInputProvider inputProvider) =>
		new(inputProvider.MouseState.X, inputProvider.MouseState.Y);

	public void Deconstruct(out MouseDirection axis)
	{
		axis = Axis;
	}
}