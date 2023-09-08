namespace CodeFirst.Utils;

public abstract class NativeResource : IDisposable
{
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
		DisposeNativeResources();
	}

	~NativeResource()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;
		DisposeNativeResources();
	}

	protected abstract void DisposeNativeResources();

	protected virtual void DisposeManagedResources()
	{
	}
}