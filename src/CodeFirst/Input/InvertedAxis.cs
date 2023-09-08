namespace CodeFirst.Input;

public class InvertedAxis : Axis
{
	public InvertedAxis(Axis axis)
	{
		Axis = axis;
	}

	public override float Value => Axis.Value != 0 ? 0 : 1;
	public Axis Axis { get; init; }

	public override void Update(IInputProvider inputProvider, float gameTime)
	{
		base.Update(inputProvider, gameTime);
		Axis.Update(inputProvider, gameTime);
	}

	public void Deconstruct(out Axis axis)
	{
		axis = Axis;
	}
}