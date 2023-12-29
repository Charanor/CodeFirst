namespace CodeFirst.Input;

public class MultiAxis : Axis
{
	private readonly IList<Axis> axes;

	public MultiAxis()
	{
		axes = new List<Axis>();
	}

	public override float Value => Math.Max(Math.Min(axes.Sum(a => a.Value), val2: 1), val2: -1);

	public override void Update(IInputProvider inputProvider, float delta)
	{
		base.Update(inputProvider, delta);
		foreach (var axis in axes)
		{
			axis.Update(inputProvider, delta);
		}
	}

	public override bool HasSameBindings(Axis other)
	{
		if (other is not MultiAxis axis)
		{
			return false;
		}

		if (axis.axes.Count != axes.Count)
		{
			return false;
		}

		for (var i = 0; i < axes.Count; i++)
		{
			if (!axes[i].HasSameBindings(axis.axes[i]))
			{
				return false;
			}
		}

		return true;
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