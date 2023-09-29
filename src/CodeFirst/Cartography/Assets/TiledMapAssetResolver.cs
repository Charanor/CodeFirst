using System.Diagnostics.CodeAnalysis;
using CodeFirst.AssetManagement;
using CodeFirst.FileSystem;
using CodeFirst.Utils.Logging;

namespace CodeFirst.Cartography.Assets;

public class TiledMapAssetResolver : IAssetResolver
{
	private static readonly ILogger Logger = LoggingManager.Get<TiledMapAssetResolver>();
	
	public bool CanLoadAssetOfType(Type type) => type == typeof(TiledMap);

	public bool TryLoadAsset(FileHandle fileHandle,[NotNullWhen(true)] out object? asset)
	{
		if (!fileHandle.HasExtension(".tmx"))
		{
			asset = default;
			return false;
		}

		try
		{
			asset = TiledMap.CreateFromFileHandle(fileHandle);
			return true;
		}
		catch(Exception e)
		{
			Logger.Error(e.ToString());
			asset = default;
			return false;
		}
	}
}