using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public class Plane
{
	private readonly float inversePlanePointsDistance;

	public Plane(Vector3 p1, Vector3 p2, Vector3 p3) : this(Vector3.Cross(p3 - p1, p2 - p1).Normalized(), p1)
	{
	}

	public Plane(Vector3 normal, float distanceFromOrigin) : this(
		normal.Normalized(),
		normal.Normalized() * distanceFromOrigin)
	{
	}

	public Plane(Vector3 normal, Vector3 pointOnPlane)
	{
		PointOnPlane = pointOnPlane;
		Normal = normal.Normalized();
		DistanceFromOrigin = Vector3.Dot(normal.Normalized(), pointOnPlane);
		A = normal.Normalized().X;
		B = normal.Normalized().Y;
		C = normal.Normalized().Z;
		D = -DistanceFromOrigin;
		inversePlanePointsDistance = 1.0f / MathF.Sqrt(A * A + B * B + C * C);
	}

	public Plane(float a, float b, float c, float d) : this(Vector3.Normalize(new Vector3(a, b, c)), d)
	{
	}

	public Vector3 Normal { get; }

	public float DistanceFromOrigin { get; }

	public float A { get; }
	public float B { get; }
	public float C { get; }
	public float D { get; }

	public Vector3 PointOnPlane { get; }

	public float EvalAtPoint(Vector3 point) => A * point.X + B * point.Y + C * point.Z + D;
	public float DistanceTo(Vector3 point) => EvalAtPoint(point) * inversePlanePointsDistance;
}