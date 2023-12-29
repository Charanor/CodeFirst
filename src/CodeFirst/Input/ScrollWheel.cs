namespace CodeFirst.Input;

public class ScrollWheel : Axis
{
	private float value = float.NaN;
	private float prevValue = float.NaN;

	public ScrollWheel(ScrollDirection direction = ScrollDirection.Vertical)
	{
		Direction = direction;
	}

	public override float Value => value;
	public ScrollDirection Direction { get; init; }

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		var currentValue = GetCurrentValue(inputProvider);
		var difference = float.IsNaN(prevValue) ? 0 : currentValue - prevValue;
		prevValue = currentValue;
		value = difference;
	}

	public override bool HasSameBindings(Axis other) => other is ScrollWheel axis && axis.Direction == Direction;

	private float GetCurrentValue(IInputProvider inputProvider) => Direction switch
	{
		ScrollDirection.Vertical => inputProvider.MouseState.Scroll.Y,
		ScrollDirection.Horizontal => inputProvider.MouseState.Scroll.X,
		_ => 0
	};

	public void Deconstruct(out ScrollDirection direction)
	{
		direction = Direction;
	}
}