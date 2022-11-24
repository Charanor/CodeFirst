namespace JXS.Graphics.Core;

public abstract class NativeResource : IDisposable
{
	public bool IsDisposed { get; set; }

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;
		DisposeManagedResources();
		DisposeNativeResources();
		GC.SuppressFinalize(this);
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