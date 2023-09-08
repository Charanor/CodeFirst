using JetBrains.Annotations;

namespace CodeFirst.AssetManagement;

public class UnloadedAssetException : Exception
{
	public UnloadedAssetException([UriString] string asset) : base($"Asset {asset} is not loaded")
	{
	}
}