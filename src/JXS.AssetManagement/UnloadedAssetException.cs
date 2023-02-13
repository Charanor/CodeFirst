using JetBrains.Annotations;

namespace JXS.AssetManagement;

public class UnloadedAssetException : Exception
{
	public UnloadedAssetException([UriString] string asset) : base($"Asset {asset} is not loaded")
	{
	}
}