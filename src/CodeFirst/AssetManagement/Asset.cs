using JetBrains.Annotations;

namespace CodeFirst.AssetManagement;

public class Asset<T> : IDisposable
{
	public Asset([UriString] string location)
	{
		Location = location;
		Assets.IncrementInstance<T>(location);
	}

	~Asset()
	{
		ReleaseUnmanagedResources();
	}

	private void ReleaseUnmanagedResources()
	{
		Assets.DecrementInstance(Location);
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	public bool IsLoaded => Assets.GetAssetState(Location) == AssetState.Loaded;

	public string Location { get; }
	public T Instance => Assets.Get<T>(Location);

	public bool TryGetInstance(out T? instance) => Assets.TryGetAsset(Location, out instance);

	public static implicit operator T(Asset<T> asset) => asset.Instance;
	public static implicit operator bool(Asset<T> asset) => asset.IsLoaded;
}