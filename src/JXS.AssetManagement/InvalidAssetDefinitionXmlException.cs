namespace JXS.AssetManagement;

public class InvalidAssetDefinitionXmlException : Exception
{
	public InvalidAssetDefinitionXmlException(string? message) : base(message)
	{
	}
}