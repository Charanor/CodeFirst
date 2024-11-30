using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public static class VectorExtensions
{
	public static Vector2 OfLength(this Vector2 vector, float length) =>
		vector.LengthSquared == 0 ? Vector2.Zero : vector.Normalized() * length;

	public static Vector2 Limit(this Vector2 vector, float maxLength)
	{
		if (maxLength <= 0)
		{
			return Vector2.Zero;
		}

		return vector.Length > maxLength ? vector.OfLength(maxLength) : vector;
	}

	public static Vector2 Contain(this Vector2 vector, Box2 bounds) => vector.Clamp(bounds.Min, bounds.Max);

	public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max) => new(
		MathHelper.Clamp(vector.X, min.X, max.X),
		MathHelper.Clamp(vector.Y, min.Y, max.Y)
	);

	public static float AngleTowards(this Vector2 vector, Vector2 other)
	{
		var denominator = MathF.Sqrt(vector.LengthSquared * other.LengthSquared);
		if (denominator <= 0.001f)
		{
			return 0;
		}

		var dot = Vector2.Dot(vector, other);
		if (float.IsNaN(dot))
		{
			return 0;
		}

		var scaledDot = MathHelper.Clamp(dot / denominator, min: -1f, max: 1f);
		return MathF.Acos(scaledDot);
	}

	public static float SignedAngleTowards(this Vector2 vector, Vector2 other)
	{
		var dot = Vector2.PerpDot(vector, other);
		if (float.IsNaN(dot))
		{
			return 0;
		}

		var angle = vector.AngleTowards(other);
		return dot >= 0 ? angle : -angle;
	}

	public static float Angle(this Vector2 vector) => AngleTowards(Vector2.UnitX, vector);
	public static float SignedAngle(this Vector2 vector) => SignedAngleTowards(Vector2.UnitX, vector);

	public static Vector2 AsAngleForVector2(this float angleRad) => MathF.SinCos(angleRad);
	public static Vector2 FromAngle(float angleRad) => MathF.SinCos(angleRad);

	public static Vector2 Rotate(this Vector2 vector, float rotationRad) => new()
	{
		X = vector.X * MathF.Cos(rotationRad) - vector.Y * MathF.Sin(rotationRad),
		Y = vector.X * MathF.Sin(rotationRad) - vector.Y * MathF.Cos(rotationRad)
	};

	public static Vector2 RotateAround(this Vector2 vector, Vector2 origin, float rotationRad) => new()
	{
		X = origin.X + (vector.X - origin.X) * MathF.Cos(rotationRad) - (vector.Y - origin.Y) * MathF.Sin(rotationRad),
		Y = origin.Y + (vector.X - origin.X) * MathF.Sin(rotationRad) - (vector.Y - origin.Y) * MathF.Cos(rotationRad)
	};

	public static Vector2 ClockwiseNormal(this Vector2 vector) => new(vector.Y, -vector.X);
	public static Vector2 CounterClockwiseNormal(this Vector2 vector) => new(-vector.Y, vector.X);

	public static Vector2 ReflectX(this Vector2 vector) => new(-vector.X, vector.Y);
	public static Vector2 ReflectY(this Vector2 vector) => new(vector.X, -vector.Y);

	public static Vector2 Project(this Vector2 vec, Vector2 axis)
	{
		var axisMagnitude = axis.LengthSquared;
		if (axisMagnitude == 0)
		{
			// This isn't very correct at all. Maybe it would be better to return a NaN vector or just throw.
			return vec;
		}

		return axis * (Vector2.Dot(vec, axis) / axisMagnitude);
	}
}