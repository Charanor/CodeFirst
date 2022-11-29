using JXS.Assets.Core;
using JXS.Graphics.Core.Assets;

namespace JXS.Graphics.Text.Assets;

public record FontAssetDefinition(string Path, TextureAssetDefinition TextureAtlasAsset) : AssetDefinition<Font>(Path);