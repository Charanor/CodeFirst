using System.Diagnostics.CodeAnalysis;
using JXS.Assets.Core.Generic;

namespace JXS.Assets.Core;

public interface IAssetLoaderProvider<TAssetType, TAssetDefinition> where TAssetDefinition : AssetDefinition<TAssetType>
{
	/// <summary>
	///     Tries to find a loader for an asset given its definition.
	/// </summary>
	/// <param name="assetDefinition">the asset definition</param>
	/// <param name="assetLoader">the loader, if any loader was found, otherwise <c>null</c></param>
	/// <returns><c>true</c> if a loader was found, <c>false</c> otherwise</returns>
	bool TryFindLoader(TAssetDefinition assetDefinition,
		[NotNullWhen(true)] out IAssetLoader<TAssetType, TAssetDefinition>? assetLoader);
}