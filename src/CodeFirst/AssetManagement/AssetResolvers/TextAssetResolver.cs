using System.Diagnostics.CodeAnalysis;
using CodeFirst.FileSystem;

namespace CodeFirst.AssetManagement.AssetResolvers;

public class TextAssetResolver : IAssetResolver
{
	public bool CanLoadAssetOfType(Type type) => type == typeof(string);

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		if (fileHandle.Type != FileType.File)
		{
			asset = default;
			return false;
		}

		asset = fileHandle.ReadAllText();
		return true;
	}
}