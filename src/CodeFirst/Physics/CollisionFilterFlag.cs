namespace CodeFirst.Physics;

/// <summary>
///		Represents an object used for filtering collisions.
/// </summary>
/// <param name="CategoryBits">The categories this filter *belongs to*. This is a bitmask.</param>
/// <param name="MaskBits">The categories this filter *collides with*. This is a bitmask.</param>
public readonly record struct CollisionFilterFlag(int CategoryBits, int MaskBits)
{
	public static readonly CollisionFilterFlag Nothing = new();
	public static readonly CollisionFilterFlag Everything = Nothing.Inverted;

	public CollisionFilterFlag Inverted => new(~CategoryBits, ~MaskBits);

	public bool CanCollideWith(CollisionFilterFlag other) => (MaskBits & other.CategoryBits) != 0;
}

public delegate CollisionResolution CollisionFilterFunction(Guid self, Guid other);