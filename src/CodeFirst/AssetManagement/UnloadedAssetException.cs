using JetBrains.Annotations;

namespace CodeFirst.AssetManagement;

public class UnloadedAssetException : Exception
{
	public UnloadedAssetException([UriString] string asset, Exception? innerException = null) : base($"Asset {asset} is not loaded", innerException)
	{
	}
}