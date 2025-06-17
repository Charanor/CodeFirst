using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public static class VectorExtensions
{
	public static Vector2 OfLength(in this Vector2 vector, float length) =>
		vector.LengthSquared == 0 ? Vector2.Zero : vector.Normalized() * length;

	public static Vector2 Limit(in this Vector2 vector, float maxLength)
	{
		if (maxLength <= 0)
		{
			return Vector2.Zero;
		}

		return vector.Length > maxLength ? vector.OfLength(maxLength) : vector;
	}

	public static Vector2 Contain(in this Vector2 vector, Box2 bounds) => vector.Clamp(bounds.Min, bounds.Max);

	public static Vector2 Clamp(in this Vector2 vector, Vector2 min, Vector2 max) => new(
		System.Math.Clamp(vector.X, min.X, max.X),
		System.Math.Clamp(vector.Y, min.Y, max.Y)
	);

	public static float AngleTowards(in this Vector2 vector, in Vector2 other) =>
		MathF.Atan2(Cross(vector, other), Vector2.Dot(vector, other));

	public static float AngleToPoint(in this Vector2 vector, in Vector2 other) => (other - vector).Angle();

	public static float Angle(in this Vector2 vector) => MathF.Atan2(vector.Y, vector.X);

	public static Vector2 FromAngle(float angleRad) => new(MathF.Cos(angleRad), MathF.Sin(angleRad));

	public static Vector2 Rotate(in this Vector2 vector, float rotationRad)
	{
		var cos = MathF.Cos(rotationRad);
		var sin = MathF.Sin(rotationRad);
		return new Vector2
		{
			X = vector.X * cos - vector.Y * sin,
			Y = vector.X * sin + vector.Y * cos
		};
	}

	public static Vector2 RotateAround(in this Vector2 vector, in Vector2 origin, float rotationRad)
	{
		var cos = MathF.Cos(rotationRad);
		var sin = MathF.Sin(rotationRad);
		return new Vector2
		{
			X = origin.X + (vector.X - origin.X) * cos - (vector.Y - origin.Y) * sin,
			Y = origin.Y + (vector.X - origin.X) * sin + (vector.Y - origin.Y) * cos
		};
	}

	public static Vector2 ClockwiseNormal(in this Vector2 vector) => new(vector.Y, -vector.X);
	public static Vector2 CounterClockwiseNormal(in this Vector2 vector) => new(-vector.Y, vector.X);

	public static Vector2 ReflectX(in this Vector2 vector) => new(-vector.X, vector.Y);
	public static Vector2 ReflectY(in this Vector2 vector) => new(vector.X, -vector.Y);

	public static Vector2 Reflect(in this Vector2 vector, in Vector2 normal) =>
		normal * Vector2.Dot(vector, normal) * 2f - vector;

	public static Vector2 Bounce(in this Vector2 vector, in Vector2 normal) => -Reflect(in vector, in normal);

	public static Vector2 Slide(in this Vector2 vector, in Vector2 normal) =>
		vector - normal * Vector2.Dot(vector, normal);

	public static Vector2 Project(in this Vector2 vector, in Vector2 axis)
	{
		var axisMagnitude = axis.LengthSquared;
		if (axisMagnitude == 0)
		{
			// This isn't very correct at all. Maybe it would be better to return a NaN vector or just throw.
			return vector;
		}

		return axis * (Vector2.Dot(vector, axis) / axisMagnitude);
	}

	public static float Cross(in this Vector2 vector, in Vector2 other) => vector.X * other.Y - vector.Y * other.X;

	public static Vector2 Round(in this Vector2 vector) => new(MathF.Round(vector.X), MathF.Round(vector.Y));
	public static Vector2 Floor(in this Vector2 vector) => new(MathF.Floor(vector.X), MathF.Floor(vector.Y));
	public static Vector2 Ceil(in this Vector2 vector) => new(MathF.Ceiling(vector.X), MathF.Ceiling(vector.Y));

	public static Vector2i RoundInt(in this Vector2 vector) =>
		new((int)MathF.Round(vector.X), (int)MathF.Round(vector.Y));

	public static Vector2i FloorInt(in this Vector2 vector) =>
		new((int)MathF.Floor(vector.X), (int)MathF.Floor(vector.Y));

	public static Vector2i CeilInt(in this Vector2 vector) =>
		new((int)MathF.Ceiling(vector.X), (int)MathF.Ceiling(vector.Y));

	public static bool Eq(in this Vector2 vector, in Vector2 other, float epsilon = 0.00001f) =>
		MathHelper.ApproximatelyEqualEpsilon(vector.X, other.X, epsilon) &&
		MathHelper.ApproximatelyEqualEpsilon(vector.Y, other.Y, epsilon);

	public static float MaxAxis(in this Vector3 vector) => MathF.Max(MathF.Max(vector.X, vector.Y), vector.Z);
	public static float MaxAxis(in this Vector2 vector) => MathF.Max(vector.X, vector.Y);

	public static int MaxAxis(in this Vector3i vector) =>
		System.Math.Max(System.Math.Max(vector.X, vector.Y), vector.Z);

	public static int MaxAxis(in this Vector2i vector) => System.Math.Max(vector.X, vector.Y);

	public static float MinAxis(in this Vector3 vector) => MathF.Min(MathF.Min(vector.X, vector.Y), vector.Z);
	public static float MinAxis(in this Vector2 vector) => MathF.Min(vector.X, vector.Y);

	public static int MinAxis(in this Vector3i vector) =>
		System.Math.Min(System.Math.Min(vector.X, vector.Y), vector.Z);

	public static int MinAxis(in this Vector2i vector) => System.Math.Min(vector.X, vector.Y);
}