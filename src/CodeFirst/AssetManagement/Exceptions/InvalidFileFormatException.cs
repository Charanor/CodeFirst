namespace CodeFirst.AssetManagement.Exceptions;

public class InvalidFileFormatException : IOException
{
	public InvalidFileFormatException(string path, string expectedType) : base(
		$"File {path} is not of a valid {expectedType} format")
	{
	}
}