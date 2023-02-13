using JXS.Async.Operations;
using JXS.Utils;
using JXS.Utils.Collections;

namespace JXS.Async;

public static class Coroutines
{
	private static readonly SnapshotList<Coroutine> DurationCoroutines = new();
	private static readonly SnapshotList<Coroutine> EventCoroutines = new();

	public static void Update(float delta)
	{
		if (!MainThread.IsRunningOnMainThread)
		{
			DevTools.ThrowStatic(new InvalidOperationException($"Can not run {nameof(Update)} from non-main thread!"));
			return;
		}

		if (DurationCoroutines.IsIterating)
		{
			DevTools.ThrowStatic(new InvalidOperationException($"{nameof(Update)} is already running!"));
			return;
		}

		using var handle = DurationCoroutines.BeginHandle(SnapshotList<Coroutine>.HandleAction.Commit);
		var (items, count) = handle;
		for (var i = 0; i < count; i++)
		{
			var coroutine = items[i];
			// The coroutine itself might take some time, so we need to take this time into account as well for the future
			delta += coroutine.Update(delta);
			if (coroutine.State != CoroutineState.Running)
			{
				DurationCoroutines.Remove(coroutine);
			}
			else if (coroutine.TickType == TickType.Event)
			{
				EventCoroutines.Add(coroutine);
				DurationCoroutines.Remove(coroutine);
			}
		}
	}

	public static void RaiseEvent(Event evt)
	{
		if (!MainThread.IsRunningOnMainThread)
		{
			// We can't raise events on non-main thread, so just post an action
			MainThread.Post(() => RaiseEventInternal(evt)).WaitWhileHandlingMainThreadTasks();
			return;
		}

		RaiseEventInternal(evt);
	}

	private static void RaiseEventInternal(Event evt)
	{
		using var handle = EventCoroutines.BeginHandle(SnapshotList<Coroutine>.HandleAction.Commit);
		var (items, count) = handle;
		for (var i = 0; i < count; i++)
		{
			var coroutine = items[i];
			// The coroutine itself might take some time, so we need to take this time into account as well for the future
			coroutine.HandleEvent(evt);
			if (coroutine.State != CoroutineState.Running)
			{
				EventCoroutines.Remove(coroutine);
			}
			else if (coroutine.TickType == TickType.Duration)
			{
				DurationCoroutines.Add(coroutine);
				EventCoroutines.Remove(coroutine);
			}
		}
	}

	public static Coroutine Start(IEnumerator<Wait?> coroutine) => MainThread.Post(() =>
	{
		var root = new Coroutine(coroutine);
		switch (root.TickType)
		{
			case TickType.Duration:
				DurationCoroutines.Add(root);
				break;
			case TickType.Event:
				EventCoroutines.Add(root);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return root;
	}).Result;

	public static Coroutine StartAfter(Wait waitFor, IEnumerator<Wait?> coroutine) =>
		Start(StartAfterRoutine(waitFor, coroutine));

	public static Coroutine StartAfter(Wait waitFor, Action action) =>
		Start(StartAfterRoutine(waitFor, action));

	private static IEnumerator<Wait?> StartAfterRoutine(Wait waitFor, IEnumerator<Wait?> coroutine)
	{
		yield return waitFor;
		yield return Start(coroutine);
	}

	private static IEnumerator<Wait?> StartAfterRoutine(Wait waitFor, Action action)
	{
		yield return waitFor;
		action();
	}

	public static Wait Wait(this float seconds) => new WaitForSeconds(seconds);
	public static Wait Wait(this Event evt) => new WaitForEvent(evt);

	/// <seealso cref="Coroutines.RaiseEvent" />
	public static void Raise(this Event evt) => RaiseEvent(evt);

	/// <seealso cref="Coroutines.Start" />
	public static void RunAsCoroutine(this IEnumerator<Wait?> coroutine) => Start(coroutine);

	public static void RunAsCoroutineAfter(this IEnumerator<Wait?> coroutine, Wait wait) => StartAfter(wait, coroutine);
}