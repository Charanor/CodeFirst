using System.Diagnostics.CodeAnalysis;

namespace JXS.Assets.Core;

public interface IAssetLoader : IDisposable
{
	Type AssetType { get; }

	bool CanLoadAsset<TAssetType>(AssetDefinition<TAssetType> assetDefinition);

	bool TryLoadAsset<TAssetType>(AssetDefinition<TAssetType> assetDefinition, [NotNullWhen(true)] out TAssetType? asset);

	bool UnloadAsset<TAssetType>(AssetDefinition<TAssetType> assetDefinition);
	bool UnloadAsset<TAssetType>(TAssetType asset);

	bool CanLoadAssetType<TAssetType>() => CanLoadAssetType(typeof(TAssetType));
	bool CanLoadAssetType(Type assetType) => assetType == AssetType;
}