namespace CodeFirst.Input;

internal class NegatedAxis : Axis
{
	private readonly Axis baseAxis;

	public NegatedAxis(Axis baseAxis)
	{
		this.baseAxis = baseAxis;
	}

	public override float Value => -baseAxis.Value;

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		baseAxis.Update(inputProvider, delta);
	}

	public override bool HasSameBindings(Axis other) => other is NegatedAxis axis && axis.baseAxis == baseAxis;
}