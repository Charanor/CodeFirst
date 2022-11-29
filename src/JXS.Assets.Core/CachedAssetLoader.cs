using System.Diagnostics.CodeAnalysis;
using JXS.Assets.Core.Exceptions;
using JXS.Assets.Core.Generic;

namespace JXS.Assets.Core;

public abstract class CachedAssetLoader<TAssetType, TAssetDefinition> : IAssetLoader<TAssetType, TAssetDefinition>,
	IAssetLoader where TAssetDefinition : AssetDefinition<TAssetType>
{
	private readonly Dictionary<string, TAssetType> assetCache;

	protected CachedAssetLoader()
	{
		assetCache = new Dictionary<string, TAssetType>();
	}

	bool IAssetLoader.CanLoadAsset<TExplicitAssetType>(AssetDefinition<TExplicitAssetType> assetDefinition)
	{
		if (assetDefinition is not AssetDefinition<TAssetType> possiblyValidAssetDefinition)
		{
			return false;
		}

		var validAssetDefinition = possiblyValidAssetDefinition as TAssetDefinition;
		return validAssetDefinition != null && CanLoadAsset(validAssetDefinition);
	}

	bool IAssetLoader.TryLoadAsset<TExplicitAssetType>(AssetDefinition<TExplicitAssetType> assetDefinition,
		[NotNullWhen(true)] out TExplicitAssetType? asset) where TExplicitAssetType : default
	{
		if (assetDefinition is not TAssetDefinition validAssetDefinition)
		{
			asset = default;
			return false;
		}

		// TODO: Maybe throw if the asset type does not extend TExplicitAssetType?
		if (TryLoadAsset(validAssetDefinition, out var typedAsset) && typedAsset is TExplicitAssetType explicitAsset)
		{
			asset = explicitAsset;
			return true;
		}

		asset = default;
		return false;
	}

	public bool UnloadAsset<TExplicitAssetType>(AssetDefinition<TExplicitAssetType> assetDefinition)
	{
		if (assetDefinition is AssetDefinition<TAssetType> validAssetDefinition)
		{
			return UnloadAsset(validAssetDefinition);
		}

		return false;
	}

	public bool UnloadAsset<TExplicitAssetType>(TExplicitAssetType asset) =>
		asset is TAssetType validAsset && UnloadAsset(validAsset);

	public Type AssetType => typeof(TAssetType);

	public bool UnloadAsset(TAssetType asset)
	{
		foreach (var (path, possibleAsset) in assetCache)
		{
			if (!Equals(asset, possibleAsset))
			{
				continue;
			}

			if (asset is IDisposable disposable)
			{
				disposable.Dispose();
			}

			assetCache.Remove(path);
			return true;
		}

		return false;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		foreach (var (_, asset) in assetCache)
		{
			if (asset is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public abstract bool CanLoadAsset(TAssetDefinition assetDefinition);

	public bool TryLoadAsset(TAssetDefinition assetDefinition, [NotNullWhen(true)] out TAssetType? asset)
	{
		if (!CanLoadAsset(assetDefinition))
		{
			asset = default;
			return false;
		}

		var assetPath = assetDefinition.Path;
		if (assetCache.TryGetValue(assetPath, out asset) && asset != null)
		{
			if (IsValidAsset(asset))
			{
				return true;
			}

			// We have a cached asset, but it is not valid anymore, so remove it from the cache and load it again
			assetCache.Remove(assetPath);
			asset = default; // Probably not needed, but to ensure that no unusable asset slips through
		}

		asset = LoadAsset(assetDefinition);
		assetCache.Add(assetPath, asset);
		return true;
	}

	public bool UnloadAsset(TAssetDefinition assetDefinition)
	{
		var assetPath = assetDefinition.Path;
		if (!assetCache.TryGetValue(assetPath, out var asset))
		{
			return false;
		}

		if (asset is IDisposable disposable)
		{
			disposable.Dispose();
		}

		assetCache.Remove(assetPath);
		return true;
	}

	/// <summary>
	///     Actually loads the asset. This is only called once it is sure that the asset can be loaded (i.e. the file exists,
	///     it is of the correct type, etc.). However it makes no guarantees that the file is in a valid format. If the asset
	///     file
	///     is in an invalid format, throw <see cref="InvalidFileFormatException" />.
	/// </summary>
	/// <param name="definition">the asset definition</param>
	/// <returns>the loaded asset</returns>
	/// <exception cref="InvalidFileFormatException">
	///     If the asset file given is not in a valid format to load with this asset
	///     loader
	/// </exception>
	[return: NotNull]
	protected abstract TAssetType LoadAsset(TAssetDefinition definition);

	/// <summary>
	///     Checks if the given asset is a valid asset instance. Should e.g. check if the asset is disposed if it is an
	///     instance of <see cref="IDisposable" />.
	/// </summary>
	/// <param name="asset">the asset to check</param>
	/// <returns><c>true</c> if the asset is still valid, <c>false</c> otherwise</returns>
	protected abstract bool IsValidAsset(TAssetType asset);
}