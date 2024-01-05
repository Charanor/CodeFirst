using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public record TextureRegion(Texture Texture, Box3i Bounds)
{
	public TextureRegion(Texture texture, Box2i bounds) : this(texture,
		new Box3i(new Vector3i(bounds.Min), new Vector3i(bounds.Max)))
	{
	}

	public TextureRegion(Texture texture, Vector2i bounds) : this(texture,
		new Box3i(new Vector3i(bounds.X, y: 0, z: 0), new Vector3i(bounds.Y, y: 0, z: 0)))
	{
	}

	public TextureRegion(TextureRegion region, int x, int y, int width, int height)
		: this(
			region.Texture,
			Box2i.FromSize(region.Bounds2D.Location + new Vector2i(x, y), (width, height))
		)
	{
	}

	public TextureRegion(TextureRegion region, Box2i bounds)
		: this(
			region.Texture,
			Box2i.FromSize(region.Bounds2D.Location + bounds.Location, bounds.Size)
		)
	{
	}

	public Box2i Bounds2D => new(Bounds.Min.Xy, Bounds.Max.Xy);
	public Vector2i Bounds1D => new(Bounds.Min.X, Bounds.Max.X);

	public Box3 UVBounds => new(Bounds.Min / (Vector3)Texture.Dimensions, Bounds.Max / (Vector3)Texture.Dimensions);

	public Box2 UVBounds2D => new(Bounds2D.Min / (Vector2)Texture.Dimensions.Xy,
		Bounds2D.Max / (Vector2)Texture.Dimensions.Xy);

	public Vector2 UVBounds1D =>
		new(Bounds1D.X / (float)Texture.Dimensions.X, Bounds1D.X / (float)Texture.Dimensions.X);

	public Vector3i Size => Bounds.Size;
	public Vector2i Size2D => Bounds2D.Size;

	public int Width => Bounds.Width;
	public int Height => Bounds.Height;
	public int Depth => Bounds.Depth;

	public static implicit operator TextureRegion(Texture texture) =>
		new(texture, Box3i.FromSize(Vector3i.Zero, texture.Dimensions));
}