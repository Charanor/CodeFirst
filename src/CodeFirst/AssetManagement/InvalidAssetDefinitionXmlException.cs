namespace CodeFirst.AssetManagement;

public class InvalidAssetDefinitionXmlException : Exception
{
	public InvalidAssetDefinitionXmlException(string? message) : base(message)
	{
	}
}