namespace JXS.AssetManagement;

public class UnresolvedAssetException : Exception
{
	public UnresolvedAssetException(string asset) : base($"No asset loaders could resolve asset {asset}")
	{
	}
}