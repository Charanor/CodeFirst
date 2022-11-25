namespace JXS.Assets.Core;

public sealed class AssetManager : IDisposable
{
	private readonly string rootDirectory;

	private readonly IDictionary<string, object> assetCache;
	private bool isDisposed;

	public AssetManager(params string[] rootDirectoryPath) : this(Path.Combine(rootDirectoryPath))
	{
	}

	public AssetManager(string rootDirectory)
	{
		this.rootDirectory = WithCorrectDirectorySeparator(rootDirectory);
		assetCache = new Dictionary<string, object>();
	}

	public void Dispose()
	{
		lock (this)
		{
			if (isDisposed)
			{
				return;
			}

			isDisposed = true;

			// Dispose managed (IDisposable) resources
			lock (assetCache)
			{
				foreach (var (_, value) in assetCache)
				{
					if (value is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
		}
	}

	public TAssetType Load<TAssetType, TAssetDefinition>(TAssetDefinition definition)
		where TAssetDefinition : AssetDefinition<TAssetType> => definition.Load(this);

	public bool Unload<TAssetType, TAssetDefinition>(TAssetDefinition definition)
		where TAssetDefinition : AssetDefinition<TAssetType> => Unload(definition.Path);

	public TAssetType Reload<TAssetType, TAssetDefinition>(TAssetDefinition definition)
		where TAssetDefinition : AssetDefinition<TAssetType>
	{
		Unload(definition);
		return Load<TAssetType, TAssetDefinition>(definition);
	}

	public bool Unload(string path)
	{
		lock (assetCache)
		{
			var rootRelativePath = FromRoot(path);
			if (!assetCache.TryGetValue(rootRelativePath, out var asset))
			{
				return false;
			}

			if (asset is IDisposable disposable)
			{
				disposable.Dispose();
			}

			assetCache.Remove(rootRelativePath);
			return true;
		}
	}

	public bool Unload(object obj)
	{
		lock (assetCache)
		{
			string? foundKey = null;
			foreach (var (key, value) in assetCache)
			{
				if (value != obj)
				{
					continue;
				}

				foundKey = key;
				break;
			}

			return foundKey != null && Unload(foundKey);
		}
	}

	public T? GetCachedAsset<T>(string path) where T : class
	{
		lock (assetCache)
		{
			return assetCache.TryGetValue(path, out var value) ? value as T : null;
		}
	}

	public bool CacheAsset<T>(string path, T value) where T : class
	{
		lock (assetCache)
		{
			if (assetCache.ContainsKey(path))
			{
				return false;
			}

			assetCache.Add(path, value);
			return true;
		}
	}

	private static string WithCorrectDirectorySeparator(string path) => path
		.Replace(oldChar: '/', Path.DirectorySeparatorChar)
		.Replace(oldChar: '\\', Path.DirectorySeparatorChar);

	public string FromRoot(string path) => Path.Combine(rootDirectory, WithCorrectDirectorySeparator(path));
}