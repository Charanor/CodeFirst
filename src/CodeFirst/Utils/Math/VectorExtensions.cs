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
}