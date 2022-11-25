namespace JXS.Assets.Core;

public abstract record AssetDefinition<TAssetType>(string Path)
{
	public abstract TAssetType Load(AssetManager manager);
}