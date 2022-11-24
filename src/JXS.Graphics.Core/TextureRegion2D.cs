using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public record TextureRegion2D(Texture2D Texture, Box2 Bounds)
{
	public Box2 UVBounds => new (Bounds.Min / Texture.Size, Bounds.Max / Texture.Size);
}