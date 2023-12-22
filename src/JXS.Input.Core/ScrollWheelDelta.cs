namespace JXS.Input.Core;

public class ScrollWheelDelta : Axis
{
	private float value;
	
	public ScrollWheelDelta(ScrollDirection direction = ScrollDirection.Vertical)
	{
		Direction = direction;
	}

	public override float Value => value;
	public ScrollDirection Direction { get; init; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		value = GetCurrentValue(inputProvider);
	}

	private float GetCurrentValue(IInputProvider inputProvider) => Direction switch
	{
		ScrollDirection.Vertical => inputProvider.MouseState.ScrollDelta.Y,
		ScrollDirection.Horizontal => inputProvider.MouseState.ScrollDelta.X,
		_ => 0
	};

	public void Deconstruct(out ScrollDirection direction)
	{
		direction = Direction;
	}
}