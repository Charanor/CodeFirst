using CodeFirst.Graphics.Core.Utils;
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

public readonly record struct Line2D(Vector2 Start, Vector2 End)
{
	// Dev note: You might be worried about division by 0 here, but that still gives the correct answer for most
	// calculations (such as AABB intersection), so it is no worries.
	/// <summary>
	///     Note that some of the components of this vector might be NaN. This is intentional and will still produce
	///     correct results in intersection tests and the like.
	/// </summary>
	public Vector2 InverseDirection { get; } = Vector2.One / (End - Start).Normalized();

	public Vector2 Direction { get; } = (End - Start).Normalized();
	public float Length { get; } = Vector2.Distance(Start, End);

	public Vector2 Midpoint { get; } = (Start + End) / 2f;

	public bool IntersectAabb(in Box2 aabb, out float distance) => Intersect.LineAabb(in this, in aabb, out distance);

	public bool IntersectCircle(in Circle circle, out float distance) =>
		Intersect.LineCircle(in this, in circle, out distance);

	public Vector2 NearestPoint(Vector2 point) =>
		Start + Direction * DistanceAlongDirection(point);

	public float DistanceAlongDirection(Vector2 point)
	{
		var originToPoint = point - Start;
		return Vector2.Dot(originToPoint, Direction).Clamp(min: 0, Length);
	}

	/// <summary>
	/// </summary>
	/// <param name="point"></param>
	/// <returns>
	///     <ul>
	///         <li>0: Point is collinear with the line (i.e. on the line).</li>
	///         <li>-1: Point is left of the line.</li>
	///         <li>1: Point is right of the line.</li>
	///     </ul>
	/// </returns>
	public int GetSide(Vector2 point)
	{
		var crossProduct = (End.X - Start.X) * (point.Y - Start.Y) - (End.Y - Start.Y) * (point.X - Start.X);
		// Positive cross product means it's on the left, but we want -1 to indicate left, so we "swap" signs.
		return crossProduct == 0 ? 0 : crossProduct > 0 ? -1 : 1;
	}

	public bool IntersectLine(in Line2D other, out Vector2 point)
	{
		const float epsilon = 1e-10f;
		
		var towardsStart = other.Start - Start;
		var r = End - Start; // We don't want to use "Direction" property here since we don't want it normalized
		var s = other.End - other.Start;
		var rXs = r.Cross(s);
		var startXr = towardsStart.Cross(r);

		if (MathF.Abs(rXs) < epsilon && MathF.Abs(startXr) < epsilon)
		{
			// Lines are collinear. For this implementation we do not consider them as intersecting.
			point = Vector2.Zero;
			return false;
		}

		if (MathF.Abs(rXs) < epsilon && MathF.Abs(startXr) >= epsilon)
		{
			// Lines are parallel
			point = Vector2.Zero;
			return false;
		}

		var t = towardsStart.Cross(s) / rXs;
		var u = towardsStart.Cross(r) / rXs;

		if (MathF.Abs(rXs) < epsilon || t is < 0 or > 1 || u is < 0 or > 1)
		{
			// No intersection
			point = Vector2.Zero;
			return false;
		}

		point = Start + r * t;
		return true;
	}
}