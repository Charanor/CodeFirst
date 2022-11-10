namespace JXS.Input.Core;

internal record NegatedAxis : Axis
{
	private readonly Axis baseAxis;

	public NegatedAxis(Axis baseAxis)
	{
		this.baseAxis = baseAxis;
	}

	public override float Value => -baseAxis.Value;

	public override void Update(float delta)
	{
		base.Update(delta);
		baseAxis.Update(delta);
	}
}