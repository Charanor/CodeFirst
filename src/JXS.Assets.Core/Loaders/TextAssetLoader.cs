using System.Text;

namespace JXS.Assets.Core.Loaders;

public class TextAssetLoader : CachedAssetLoader<TextAsset, TextAssetDefinition>
{
	public override bool CanLoadAsset(TextAssetDefinition assetDefinition)
	{
		var path = assetDefinition.Path;
		return File.Exists(path);
	}

	protected override TextAsset LoadAsset(TextAssetDefinition definition) =>
		new(File.ReadAllText(definition.Path, definition.Encoding));

	protected override bool IsValidAsset(TextAsset asset) => true;
}

public record TextAssetDefinition(string Path) : AssetDefinition<TextAsset>(Path)
{
	public Encoding Encoding { get; init; } = Encoding.Default;
}

public record TextAsset(string Text);