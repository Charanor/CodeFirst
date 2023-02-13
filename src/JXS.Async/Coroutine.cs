namespace JXS.Async;

public sealed class Coroutine : Wait
{
	private readonly IEnumerator<Wait?> operations;

	internal Coroutine(IEnumerator<Wait?> operations)
	{
		this.operations = operations;

		// Run the 1st thing immediately
		if (!operations.MoveNext())
		{
			Finish();
		}
	}

	private Wait? CurrentOperation => operations.Current;

	public override TickType TickType => CurrentOperation?.TickType ?? TickType.Duration;

	protected override void UpdateInternal(float delta)
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		if (CurrentOperation == null)
		{
			ExecuteNextOperation();
		}
		else
		{
			CurrentOperation.Update(delta);
			if (CurrentOperation.State != CoroutineState.Running)
			{
				ExecuteNextOperation();
			}
		}
	}

	public override void HandleEvent(Event evt)
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		if (CurrentOperation == null)
		{
			ExecuteNextOperation();
		}
		else
		{
			CurrentOperation.HandleEvent(evt);
			if (CurrentOperation.State != CoroutineState.Running)
			{
				ExecuteNextOperation();
			}
		}
	}

	private void ExecuteNextOperation()
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		if (!operations.MoveNext())
		{
			Finish();
		}
	}

	public override void Cancel()
	{
		if (State != CoroutineState.Running)
		{
			return;
		}

		base.Cancel();
		CurrentOperation?.Cancel(); // This is needed for cases like nested coroutines etc.
	}
}