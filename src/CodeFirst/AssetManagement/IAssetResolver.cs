using System.Diagnostics.CodeAnalysis;
using CodeFirst.FileSystem;

namespace CodeFirst.AssetManagement;

public interface IAssetResolver
{
	bool CanLoadAssetOfType(Type type);
	bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset);
}