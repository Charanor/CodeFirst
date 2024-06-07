using System.Collections.Concurrent;
using CodeFirst.Windowing;

namespace CodeFirst.Utils;

/// <summary>
///     Utility methods to run actions on the main thread. Especially useful for initializing OpenGL objects which must
///		be done on the GL context thread (often the main thread).
/// </summary>
/// <seealso cref="MainThreadInstance"/>
/// <seealso cref="RunPostedActions"/>
public static class MainThread
{
	private static readonly ConcurrentBag<Task> PostedTasks = new();

	/// <summary>
	///		When your program is created, set this to the current thread <c>MainThread.MainThreadInstance = Thread.CurrentThread;</c>.
	///		Make sure to call <see cref="RunPostedActions"/> on this thread and only this thread!
	/// </summary>
	/// <remarks>
	///		You can of course set this to whatever thread you want, so if you are using this to instantiate OpenGL objects
	///		and your "main" thread is not the OpenGL context thread, you can set this to the OpenGL context thread instead.
	/// </remarks>
	/// <example>MainThread.MainThreadInstance = Thread.CurrentThread;</example>
	public static Thread? MainThreadInstance { get; set; }

	/// <summary>
	///		Is <c>true</c> if we are currently on the main thread, <c>false</c> otherwise.
	/// </summary>
	public static bool IsRunningOnMainThread => Thread.CurrentThread == MainThreadInstance;

	/// <summary>
	///		Posts the given action to run on the main thread and returns a task that will finish after the action is ran.
	///		If the current thread is already the main thread (see <see cref="IsRunningOnMainThread"/>) the action will
	///		be run immediately.
	/// </summary>
	/// <param name="action">the action to run</param>
	/// <typeparam name="T">the return value of the action</typeparam>
	/// <returns>A <see cref="Task{T}"/></returns>
	/// <exception cref="NullReferenceException"></exception>
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

	/// <summary>
	///		Posts the given action to run on the main thread and returns a task that will finish after the action is ran.
	///		If the current thread is already the main thread (see <see cref="IsRunningOnMainThread"/>) the action will
	///		be run immediately.
	/// </summary>
	/// <param name="action">the action to run</param>
	/// <exception cref="NullReferenceException"></exception>
	/// <returns>A <see cref="Task"/></returns>
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

	/// <summary>
	///		Runs all tasks posted by <see cref="Post{T}"/> and <see cref="Post"/>. If this method is called on a thread
	///		other than the main thread it will do nothing, so make sure to call it on the same thread as was assigned
	///		to <see cref="MainThreadInstance"/>.
	/// </summary>
	/// <remarks>
	///		If using this primarily for creating OpenGL objects it is recommended to call this during the Draw/Render
	///		frame instead of the Update frame, e.g. <see cref="Game.OnRenderFrame">Game.OnRenderFrame</see>.
	///	</remarks>
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

	/// <summary>
	///		Sometimes you will call <see cref="Post{T}"/> with an action that also calls <see cref="Post{T}"/> (e.g. instantiating
	///		a mesh that internally instantiates a texture). In cases like this you can enter a deadlock when calling
	///		<see cref="Task.Wait()">Task.Wait()</see> on the task returned from <see cref="Post{T}"/>. In cases like
	///		this, use this extension method instead to prevent deadlocks.
	/// </summary>
	/// <example>
	/// <code>
	///		var task = MainThread.Post(() => new Mesh(...));
	///		task.WaitWhileHandlingMainThreadTasks();
	///		meshes.Add(task.Result);
	/// </code>
	/// </example>
	/// <param name="task"></param>
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