using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public record TextureRegion(Texture Texture, Box3 Bounds)
{
	public TextureRegion(Texture texture, Box2 bounds) : this(texture,
		new Box3(new Vector3(bounds.Min), new Vector3(bounds.Max)))
	{
	}

	public TextureRegion(Texture texture, Vector2 bounds) : this(texture,
		new Box3(new Vector3(bounds.X, y: 0, z: 0), new Vector3(bounds.Y, y: 0, z: 0)))
	{
	}

	public Box2 Bounds2D => new(Bounds.Min.Xy, Bounds.Max.Xy);
	public Vector2 Bounds1D => new(Bounds.Min.X, Bounds.Max.X);

	public Box3 UVBounds3D => new(Bounds.Min / Texture.Dimensions, Bounds.Max / Texture.Dimensions);
	public Box2 UVBounds2D => new(Bounds2D.Min / Texture.Dimensions.Xy, Bounds2D.Max / Texture.Dimensions.Xy);
	public Vector2 UVBounds1D => new(Bounds1D.X / Texture.Dimensions.X, Bounds1D.X / Texture.Dimensions.X);

	public static implicit operator TextureRegion(Texture texture) =>
		new(texture, Box3.FromSize(Vector3.Zero, texture.Dimensions));
}