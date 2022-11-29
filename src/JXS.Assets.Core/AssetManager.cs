using System.Diagnostics.CodeAnalysis;
using JXS.Assets.Core.Loaders;

namespace JXS.Assets.Core;

public sealed class AssetManager : IDisposable
{
	private readonly List<IAssetLoader> assetLoaders;

	private bool isDisposed;

	public AssetManager()
	{
		assetLoaders = new List<IAssetLoader> { new TextAssetLoader() };
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
			lock (assetLoaders)
			{
				assetLoaders.ForEach(loader => loader.Dispose());
			}
		}
	}

	public void AddAssetLoader(IAssetLoader assetLoader)
	{
		lock (assetLoaders)
		{
			assetLoaders.Add(assetLoader);
		}
	}

	public void RemoveAssetLoader(IAssetLoader assetLoader)
	{
		lock (assetLoaders)
		{
			assetLoaders.Remove(assetLoader);
		}
	}

	public bool TryLoadAsset<TAssetType>(AssetDefinition<TAssetType> definition,
		[NotNullWhen(true)] out TAssetType? asset)
	{
		lock (assetLoaders)
		{
			foreach (var assetLoader in assetLoaders)
			{
				if (assetLoader.TryLoadAsset(definition, out asset))
				{
					return true;
				}
			}

			asset = default;
			return false;
		}
	}

	public bool CanLoadAsset<TAssetType>(AssetDefinition<TAssetType> assetDefinition)
	{
		lock (assetLoaders)
		{
			return assetLoaders.Any(assetLoader => assetLoader.CanLoadAsset(assetDefinition));
		}
	}

	public bool CanLoadAssetType<TAssetType>()
	{
		lock (assetLoaders)
		{
			return assetLoaders.Any(assetLoader => assetLoader.CanLoadAssetType<TAssetType>());
		}
	}

	public bool CanLoadAssetType(Type assetType)
	{
		lock (assetLoaders)
		{
			return assetLoaders.Any(assetLoader => assetLoader.CanLoadAssetType(assetType));
		}
	}

	public bool Reload<TAssetType, TAssetDefinition>(TAssetDefinition definition,
		[NotNullWhen(true)] out TAssetType? asset)
		where TAssetDefinition : AssetDefinition<TAssetType>
	{
		Unload(definition);
		return TryLoadAsset(definition, out asset);
	}

	public bool Unload<TAssetType, TAssetDefinition>(TAssetDefinition definition)
		where TAssetDefinition : AssetDefinition<TAssetType>
	{
		lock (assetLoaders)
		{
			return assetLoaders.Any(assetLoader => assetLoader.UnloadAsset(definition));
		}
	}

	public bool Unload<T>(T obj)
	{
		lock (assetLoaders)
		{
			return assetLoaders.Any(assetLoader => assetLoader.UnloadAsset(obj));
		}
	}

	private static string WithCorrectDirectorySeparator(string path) => path
		.Replace(oldChar: '/', Path.DirectorySeparatorChar)
		.Replace(oldChar: '\\', Path.DirectorySeparatorChar);
}