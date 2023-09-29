using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
	// Dev note: You might be worried about division by 0 here, but that still gives the correct answer for most
	// calculations (such as AABB intersection), so it is no worries.
	/// <summary>
	///     Note that some of the components of this vector might be NaN. This is intentional and will still produce
	///     correct results in intersection tests and the like.
	/// </summary>
	public Vector3 InverseDirection { get; } = Vector3.One / Direction.Normalized();

	public Vector3 Direction { get; } = Direction.Normalized();

	public bool IntersectPlane(in Plane plane, out float distance) =>
		Intersect.RayPlane(in this, in plane, out distance);

	public bool IntersectAabb(in Box3 aabb, out float distance) => Intersect.RayAabb(in this, in aabb, out distance);

	public bool IntersectSphere(in Sphere sphere, out float distance) =>
		Intersect.RaySphere(in this, in sphere, out distance);

	public Vector3 NearestPoint(Vector3 point) =>
		Origin + Direction * DistanceAlongDirection(point);

	public float DistanceAlongDirection(Vector3 point)
	{
		var originToPoint = point - Origin;
		return Vector3.Dot(originToPoint, Direction);
	}
}