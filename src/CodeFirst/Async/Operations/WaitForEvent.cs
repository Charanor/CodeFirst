namespace CodeFirst.Async.Operations;

internal class WaitForEvent : Wait
{
	private readonly Event waitForEvent;

	public WaitForEvent(Event evt)
	{
		waitForEvent = evt;
	}

	public override TickType TickType => TickType.Event;

	protected override void UpdateInternal(float delta)
	{
	}

	public override void HandleEvent(Event incomingEvent)
	{
		if (incomingEvent == waitForEvent)
		{
			Finish();
		}
	}

	public static implicit operator WaitForEvent(Event evt) => new(evt);
}