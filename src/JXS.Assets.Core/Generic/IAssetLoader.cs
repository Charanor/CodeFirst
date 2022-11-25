using System.Diagnostics.CodeAnalysis;

namespace JXS.Assets.Core.Generic;

public interface IAssetLoader<TAssetType, in TAssetDefinition> : IDisposable
	where TAssetDefinition : AssetDefinition<TAssetType>
{
	Type AssetType => typeof(TAssetType);

	bool CanLoadAsset(TAssetDefinition assetDefinition);

	bool TryLoadAsset(TAssetDefinition assetDefinition, [NotNullWhen(true)] out TAssetType? asset);

	bool UnloadAsset(TAssetDefinition assetDefinition);
	bool UnloadAsset(TAssetType asset);

	bool CanLoadAssetType(Type assetType) => assetType == AssetType;
}