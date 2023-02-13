namespace JXS.AssetManagement;

public class WrongAssetTypeException : Exception
{
	public WrongAssetTypeException(string asset, Type expectedType, Type? actualType) : base(
		$"Asset {asset} is of type {actualType}, but expected it to be of type {expectedType}")
	{
	}
}