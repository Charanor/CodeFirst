using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public readonly record struct Ray2D(Vector2 Origin, Vector2 Direction)
{
	// Dev note: You might be worried about division by 0 here, but that still gives the correct answer for most
	// calculations (such as AABB intersection), so it is no worries.
	/// <summary>
	///     Note that some of the components of this vector might be NaN. This is intentional and will still produce
	///     correct results in intersection tests and the like.
	/// </summary>
	public Vector2 InverseDirection { get; } = Vector2.One / Direction.Normalized();

	public Vector2 Direction { get; } = Direction.Normalized();

	public bool IntersectAabb(in Box2 aabb, out float distance) => Intersect.RayAabb(in this, in aabb, out distance);

	public bool IntersectCircle(in Circle circle, out float distance) =>
		Intersect.RayCircle(in this, in circle, out distance);

	public Vector2 NearestPoint(Vector2 point) =>
		Origin + Direction * DistanceAlongDirection(point);

	public float DistanceAlongDirection(Vector2 point)
	{
		var originToPoint = point - Origin;
		return Vector2.Dot(originToPoint, Direction);
	}
}