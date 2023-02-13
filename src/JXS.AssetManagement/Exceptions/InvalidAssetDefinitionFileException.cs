namespace JXS.AssetManagement.Exceptions;

public class InvalidAssetDefinitionFileException : Exception
{
	public InvalidAssetDefinitionFileException(Type assetDefinitionType) : base(CreateMessage(assetDefinitionType))
	{
	}

	public InvalidAssetDefinitionFileException(Type assetDefinitionType, Exception? innerException) : base(
		CreateMessage(assetDefinitionType), innerException)
	{
	}

	private static string CreateMessage(Type type) =>
		$"Could not instantiate AssetDefinition class {type}; the type should contain a constructor that takes a single {typeof(string)} as argument.";
}