using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public static class Intersect
{
	private const float REALLY_SMALL = 0.0001f;

	public static bool RayAabb(in Ray ray, in Box3 aabb, out float distance)
	{
		var tMin = 0f;
		var tMax = float.PositiveInfinity;

		CalculateT(ray.Origin.X, ray.InverseDirection.X, aabb.Min.X, aabb.Max.X);
		CalculateT(ray.Origin.Y, ray.InverseDirection.Y, aabb.Min.Y, aabb.Max.Y);
		CalculateT(ray.Origin.Z, ray.InverseDirection.Z, aabb.Min.Z, aabb.Max.Z);

		distance = tMin;
		return tMin > 0 && tMin <= tMax;

		void CalculateT(float rayOrigin, float rayInverseDirection, float aabbMin, float aabbMax)
		{
			var t1 = (aabbMin - rayOrigin) * rayInverseDirection;
			var t2 = (aabbMax - rayOrigin) * rayInverseDirection;

			tMin = Min(Max(t1, tMin), Max(t2, tMin));
			tMax = Max(Min(t1, tMax), Min(t2, tMax));
		}
	}

	// NOTE: We can not use Math{F} built-in Min/Max functions, since they do not play nice with NaN
	private static float Min(float x, float y) => x < y ? x : y;
	private static float Max(float x, float y) => x > y ? x : y;

	/// <summary>
	///     Attempt to intersect the given ray with the given plane.
	/// </summary>
	/// <param name="ray"></param>
	/// <param name="plane"></param>
	/// <param name="distance">
	///     Where along the ray the intersection lies. Intersection point can then be calculated as
	///     <c>point = ray.Origin + ray.Direction * distance</c>. If there was no intersection, distance is <c>NaN</c>.
	///     If distance is <c>0</c> the origin of the ray lies on the plane.
	/// </param>
	/// <returns><c>true</c> if there was an intersection, <c>false</c> otherwise</returns>
	public static bool RayPlane(in Ray ray, in Plane plane, out float distance)
	{
		var denominator = Vector3.Dot(plane.Normal, ray.Direction);
		if (MathF.Abs(denominator) <= REALLY_SMALL)
		{
			distance = float.NaN;
			return false;
		}

		distance = Vector3.Dot(plane.PointOnPlane - ray.Origin, plane.Normal) / denominator;
		return distance >= 0;
	}

	public static bool RaySphere(in Ray ray, in Sphere sphere, out float distance)
	{
		var towardsRay = ray.Origin - sphere.Center;
		var raySphereAngle = Vector3.Dot(towardsRay, ray.Direction);
		var sizeDiff = Vector3.Dot(towardsRay, towardsRay) - sphere.Radius * sphere.Radius;

		// If ray's origin is outside of the sphere (denoted by sizeDiff > 0) and the ray is pointing away from the
		// sphere (denoted by raySphereAngle > 0) then we are not intersecting the sphere.
		if (sizeDiff > 0.0f && raySphereAngle > 0.0f)
		{
			distance = 0;
			return false;
		}

		// A negative discriminant corresponds to ray missing sphere 
		var discriminant = raySphereAngle * raySphereAngle - sizeDiff;
		if (discriminant < 0.0f)
		{
			distance = 0;
			return false;
		}

		// Ray intersects the sphere, compute smallest distance to intersection
		distance = -(raySphereAngle + MathF.Sqrt(discriminant));

		// If distance is negative the ray origin is inside the sphere (just clamp to zero, negative distance makes no sense) 
		distance = MathF.Max(distance, y: 0);
		return true;
	}
}