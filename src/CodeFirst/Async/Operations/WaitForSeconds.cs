namespace CodeFirst.Async.Operations;

internal class WaitForSeconds : Wait
{
	private float duration;

	public WaitForSeconds(float seconds)
	{
		Duration = seconds;
	}

	private float Duration
	{
		get => duration;
		set
		{
			duration = value;
			if (duration <= 0)
			{
				Finish();
			}
		}
	}

	public override TickType TickType => TickType.Duration;

	protected override void UpdateInternal(float delta)
	{
		Duration -= delta;
	}

	public override void HandleEvent(Event incomingEvent)
	{
	}

	public static implicit operator WaitForSeconds(float seconds) => new(seconds);
}