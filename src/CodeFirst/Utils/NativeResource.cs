namespace CodeFirst.Utils;

/// <summary>
///		A small wrapper around <see cref="IDisposable"/> meant to be used with native resources, e.g. OpenGL references.
///		Ensures that <see cref="DisposeNativeResources"/> is called on the main thread (see <see cref="MainThread"/>)
///		to prevent things like OpenGL errors when an object is GC:d on a thread without OpenGL context.
/// </summary>
public abstract class NativeResource : IDisposable
{
	/// <summary>
	///		Checks if this native resource has been disposed of yet.
	/// </summary>
	public bool IsDisposed { get; private set; }

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;
        GC.SuppressFinalize(this);
        DisposeManagedResources();
        _ = MainThread.Post(DisposeNativeResources);
	}

	~NativeResource()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;
		_ = MainThread.Post(DisposeNativeResources);
	}

	/// <summary>
	///		Dispose native resources in here, e.g. OpenGL objects.
	/// </summary>
	protected abstract void DisposeNativeResources();

	/// <summary>
	///		Dispose of <see cref="IDisposable"/>s in here.
	/// </summary>
	protected virtual void DisposeManagedResources()
	{
	}
}