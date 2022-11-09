namespace JXS.Input.Core;

public record MultiAxis : Axis
{
	private readonly IList<Axis> axes;

	public MultiAxis()
	{
		axes = new List<Axis>();
	}

	public override float Value => Math.Max(Math.Min(axes.Sum(a => a.Value), 1), -1);

	public override void Update(float delta)
	{
		base.Update(delta);
		foreach (var axis in axes) axis.Update(delta);
	}

	public static MultiAxis operator +(MultiAxis multiAxis, Axis axis)
	{
		multiAxis.axes.Add(axis);
		return multiAxis;
	}

	public static MultiAxis operator -(MultiAxis multiAxis, Axis axis)
	{
		multiAxis.axes.Remove(axis);
		return multiAxis;
	}
}