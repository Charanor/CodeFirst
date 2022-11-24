namespace JXS.Gui.Animation;

public record TimedAnimatedValue(float TargetValue, float Delay) : AnimatedValue(TargetValue)
{
	public bool OvershootClamping { get; init; }

	protected override float CalculateValueAtTime(float timeSinceStart)
	{
		var newValue = StartValue + (TargetValue - StartValue) * Delay / timeSinceStart;
		if (OvershootClamping)
		{
			newValue = TargetValue < 0
				? Math.Max(TargetValue, newValue)
				: Math.Min(TargetValue, newValue);
		}

		return newValue;
	}
}