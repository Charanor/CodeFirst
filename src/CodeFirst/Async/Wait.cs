using System.Diagnostics;
using CodeFirst.Async.Operations;

namespace CodeFirst.Async;

public abstract class Wait
{
	private static readonly ThreadLocal<Stopwatch> Stopwatch = new(() => new Stopwatch());

	public CoroutineState State { get; private set; } = CoroutineState.Running;

	public abstract TickType TickType { get; }

	public float Update(float delta)
	{
		Stopwatch.Value ??= new Stopwatch();
		Stopwatch.Value.Restart();
		UpdateInternal(delta);
		return (float)Stopwatch.Value.Elapsed.TotalSeconds;
	}

	protected abstract void UpdateInternal(float delta);
	public abstract void HandleEvent(Event incomingEvent);

	protected void Finish()
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		State = CoroutineState.Finished;
	}

	public virtual void Cancel()
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		State = CoroutineState.Cancelled;
	}

	public static Wait ForSeconds(float seconds) => new WaitForSeconds(seconds);
	public static Wait ForEvent(Event evt) => new WaitForEvent(evt);

	public static implicit operator Wait(float seconds) => ForSeconds(seconds);
	public static implicit operator Wait(Event evt) => ForEvent(evt);
}

public enum TickType
{
	Duration,
	Event
}