using JXS.AssetManagement;

namespace JXS.Graphics.Core.Assets;

public record TextureAssetDefinition : AssetDefinition
{
	public int MipMapLevels { get; init; } = 1;
	public TextureMinFilter MinFilter { get; init; } = TextureMinFilter.Linear;
	public TextureMagFilter MagFilter { get; init; } = TextureMagFilter.Linear;

	public TextureWrapMode WrapS { get; init; } = TextureWrapMode.Repeat;
	public TextureWrapMode WrapT { get; init; } = TextureWrapMode.Repeat;

	public void Deconstruct(out int mipMapLevels, out TextureMinFilter minFilter, out TextureMagFilter magFilter,
		out TextureWrapMode wrapS, out TextureWrapMode wrapT)
	{
		mipMapLevels = MipMapLevels;
		minFilter = MinFilter;
		magFilter = MagFilter;
		wrapS = WrapS;
		wrapT = WrapT;
	}
}