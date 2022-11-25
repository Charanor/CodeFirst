using JXS.Assets.Core;
using JXS.Graphics.Core;
using OpenTK.Graphics.OpenGL;

namespace JXS.Graphics.Utils.Assets;

public record TextureAssetDefinition(string Path) : AssetDefinition<Texture>(Path)
{
	public int MipMapLevels { get; init; } = 1;
	public TextureMinFilter MinFilter { get; init; } = TextureMinFilter.Linear;
	public TextureMagFilter MagFilter { get; init; } = TextureMagFilter.Linear;

	public void Deconstruct(out string path, out int mipMapLevels, out TextureMinFilter minFilter, out TextureMagFilter magFilter)
	{
		path = Path;
		mipMapLevels = MipMapLevels;
		minFilter = MinFilter;
		magFilter = MagFilter;
	}
}