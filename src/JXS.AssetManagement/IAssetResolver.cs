using System.Diagnostics.CodeAnalysis;
using JXS.FileSystem;

namespace JXS.AssetManagement;

public interface IAssetResolver
{
	bool CanLoadAssetOfType(Type type);
	bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset);
}