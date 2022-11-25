using JXS.Assets.Core;
using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;

namespace JXS.Graphics.Utils.Assets;

public record TextureAssetDefinition(string Path) : AssetDefinition<Texture>(Path)
{
	public int MipMapLevels { get; init; } = 1;
	public TextureMinFilter MinFilter { get; init; } = TextureMinFilter.Linear;
	public TextureMagFilter MagFilter { get; init; } = TextureMagFilter.Linear;

	public override Texture Load(AssetManager manager) => manager.LoadTexture(Path, MipMapLevels, MinFilter, MagFilter);
}