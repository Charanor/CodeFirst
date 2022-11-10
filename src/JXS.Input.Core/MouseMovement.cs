using OpenTK.Mathematics;

namespace JXS.Input.Core;

public record MouseMovement : Axis
{
	private Vector2 prevValue;
	private Vector2 value;

	public MouseMovement(MouseDirection axis)
	{
		Axis = axis;
		prevValue = GetMousePosition();
		value = prevValue;
	}

	public MouseDirection Axis { get; }

	public override float Value => Axis switch
	{
		MouseDirection.Vertical => value.Y,
		MouseDirection.Horizontal => value.X,
		MouseDirection.Both => value.Length,
		_ => throw new ArgumentOutOfRangeException()
	};

	public override void Update(float delta)
	{
		base.Update(delta);
		var currentValue = GetMousePosition();
		var difference = currentValue - prevValue;
		prevValue = currentValue;
		value = difference;
	}

	private Vector2 GetMousePosition() => new(MouseState.X, MouseState.Y);
}