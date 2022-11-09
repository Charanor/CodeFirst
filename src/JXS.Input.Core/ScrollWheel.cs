namespace JXS.Input.Core;

public record ScrollWheel : Axis
{
	private float value;
	private float prevValue;

	public ScrollWheel(ScrollDirection direction = ScrollDirection.Vertical)
	{
		Direction = direction;
		value = GetCurrentValue();
		prevValue = value;
	}

	public ScrollDirection Direction { get; }

	public override float Value => value;

	public override void Update(float delta)
	{
		base.Update(delta);
		var currentValue = GetCurrentValue();
		var difference = currentValue - prevValue;
		prevValue = currentValue;
		value = difference;
	}

	private float GetCurrentValue() => Direction switch
	{
		ScrollDirection.Vertical => MouseState.Scroll.Y,
		ScrollDirection.Horizontal => MouseState.Scroll.X,
		_ => 0
	};
}