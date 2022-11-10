using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public abstract class Camera
{
	private const float REALLY_SMALL = 0.0000001f;

	protected Camera(float worldWidth, float worldHeight)
	{
		WorldSize = new Vector2(worldWidth, worldHeight);
	}

	public Vector3 Position { get; set; }
	public Vector3 Forward { get; set; } = -Vector3.UnitZ;
	public Vector3 Up { get; set; } = Vector3.UnitY;
	public Vector3 Right => Vector3.Cross(Forward, Up);

	public Vector2 WindowSize { get; private set; }
	public Vector2 WorldSize { get; set; }

	public float NearClippingPlane { get; set; } = 1;
	public float FarClippingPlane { get; set; } = 100;

	public Matrix4 Combined => Projection * View;

	public abstract Matrix4 Projection { get; }
	public abstract Matrix4 View { get; }
	
	protected abstract void UpdateViewport(int windowWidth, int windowHeight);

	public void Update(int newWidth, int newHeight)
	{
		WindowSize = new Vector2(newWidth, newHeight);
	}

	public void LookAt(Vector3 point)
	{
		var newForward = Vector3.Normalize(point - Position);
		if (newForward.LengthSquared < REALLY_SMALL)
		{
			return;
		}

		var dot = Vector3.Dot(newForward, Up);
		if (MathF.Abs(dot - 1) < REALLY_SMALL)
		{
			Up = -Forward;
		}
		else if (MathF.Abs(dot + 1) < REALLY_SMALL)
		{
			Up = Forward;
		}

		Forward = newForward;
		NormalizeUp();
	}

	public void NormalizeUp()
	{
		Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
	}

	public void Rotate(Vector3 axis, float angleRadians)
	{
		Forward = Vector3.Transform(Forward, Quaternion.FromAxisAngle(axis, angleRadians));
		Up = Vector3.Transform(Up, Quaternion.FromAxisAngle(axis, angleRadians)).Normalized();
	}

	public void RotateAround(Vector3 point, Vector3 axis, float angleRadians)
	{
		var offset = point - Position;
		Position += offset;
		Rotate(axis, angleRadians);
		Position -= Vector3.Transform(offset, Quaternion.FromAxisAngle(axis, angleRadians));
	}

	// public Ray ScreenPointToRay(Vector2 point)
	// {
	// 	var nearPoint =
	// 		latestViewport.Unproject(point.AsXY(latestViewport.MinDepth), Projection, View, Matrix4.Identity);
	// 	var farPoint =
	// 		latestViewport.Unproject(point.AsXY(latestViewport.MaxDepth), Projection, View, Matrix4.Identity);
	// 	var direction = (farPoint - nearPoint).Normalized();
	// 	return new Ray(nearPoint, direction);
	// }
}