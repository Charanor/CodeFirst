using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core.Utils;

public static class MathUtils
{
	public static Quaternion QuaternionLookRotation(Vector3 forward, Vector3 up)
	{
		var vector = Vector3.Normalize(forward);
		var vector2 = Vector3.Normalize(Vector3.Cross(up, vector));

		var (m00, m01, m02) = vector2;
		var (m10, m11, m12) = Vector3.Cross(vector, vector2);
		var (m20, m21, m22) = vector;

		var num8 = m00 + m11 + m22;
		var quaternion = new Quaternion();
		if (num8 > 0f)
		{
			var num = MathF.Sqrt(num8 + 1f);
			quaternion.W = num * 0.5f;
			num = 0.5f / num;
			quaternion.X = (m12 - m21) * num;
			quaternion.Y = (m20 - m02) * num;
			quaternion.Z = (m01 - m10) * num;
			return quaternion;
		}

		if (m00 >= m11 && m00 >= m22)
		{
			var num7 = MathF.Sqrt(1f + m00 - m11 - m22);
			var num4 = 0.5f / num7;
			quaternion.X = 0.5f * num7;
			quaternion.Y = (m01 + m10) * num4;
			quaternion.Z = (m02 + m20) * num4;
			quaternion.W = (m12 - m21) * num4;
			return quaternion;
		}

		if (m11 > m22)
		{
			var num6 = MathF.Sqrt(1f + m11 - m00 - m22);
			var num3 = 0.5f / num6;
			quaternion.X = (m10 + m01) * num3;
			quaternion.Y = 0.5f * num6;
			quaternion.Z = (m21 + m12) * num3;
			quaternion.W = (m20 - m02) * num3;
			return quaternion;
		}

		var num5 = MathF.Sqrt(1f + m22 - m00 - m11);
		var num2 = 0.5f / num5;
		quaternion.X = (m20 + m02) * num2;
		quaternion.Y = (m21 + m12) * num2;
		quaternion.Z = 0.5f * num5;
		quaternion.W = (m01 - m10) * num2;
		return quaternion;
	}

	public static Quaternion QuaternionFromToRotation(Vector3 from, Vector3 to)
	{
		var axis = Vector3.Cross(from, to).Normalized();
		var angle = Vector3.CalculateAngle(from, to);
		return Quaternion.FromAxisAngle(axis, angle);
	}
}