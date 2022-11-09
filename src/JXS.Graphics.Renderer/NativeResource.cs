namespace JXS.Graphics.Renderer;

public abstract class NativeResource
{
	private bool isDisposed;

	~NativeResource()
	{
		if (isDisposed)
		{
			return;
		}

		isDisposed = true;
		DisposeNativeResources();
	}

	public void Dispose()
	{
		if (isDisposed)
		{
			return;
		}

		isDisposed = true;
		DisposeManagedResources();
		DisposeNativeResources();
		GC.SuppressFinalize(this);
	}

	protected abstract void DisposeNativeResources();

	protected virtual void DisposeManagedResources()
	{
	}
}