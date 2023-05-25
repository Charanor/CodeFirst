using System.Collections.Concurrent;

namespace JXS.Utils;

/// <summary>
///     Utility methods to run actions on the main thread. Especially useful for initializing OpenGL objects.
/// </summary>
public static class MainThread
{
	private static readonly ConcurrentBag<Task> PostedTasks = new();

	public static Thread? MainThreadInstance { get; set; }

	public static bool IsRunningOnMainThread => Thread.CurrentThread == MainThreadInstance;

	public static async Task<T> Post<T>(Func<T> action)
	{
		if (IsRunningOnMainThread)
		{
			return action();
		}

		if (MainThreadInstance == null)
		{
			throw new NullReferenceException(
				$"{nameof(MainThreadInstance)} is null! Please set this when the application starts.");
		}

		var task = new Task<T>(action);
		lock (PostedTasks)
		{
			PostedTasks.Add(task);
		}

		return await task;
	}

	public static async Task Post(Action action)
	{
		if (IsRunningOnMainThread)
		{
			action();
			return;
		}

		if (MainThreadInstance == null)
		{
			throw new NullReferenceException(
				$"{nameof(MainThreadInstance)} is null! Please set this when the application starts.");
		}

		var task = new Task(action);
		lock (PostedTasks)
		{
			PostedTasks.Add(task);
		}

		await task;
	}

	public static void RunPostedActions()
	{
		if (!IsRunningOnMainThread)
		{
			return;
		}

		lock (PostedTasks)
		{
			foreach (var postedTask in PostedTasks)
			{
				postedTask.RunSynchronously();
			}

			PostedTasks.Clear();
		}
	}

	public static void WaitWhileHandlingMainThreadTasks(this Task task)
	{
		if (!IsRunningOnMainThread)
		{
			task.Wait();
			return;
		}

		RunPostedActions();
		while (!task.Wait(1))
		{
			RunPostedActions();
		}
	}
}