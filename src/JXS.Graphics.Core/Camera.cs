using JXS.Graphics.Core.Utils;
using JXS.Utils.Math;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public abstract class Camera
{
	private const float REALLY_SMALL = 0.0000001f;

	protected static readonly IReadOnlyList<Vector3> UntransformedPlanePoints = new[]
	{
		new Vector3(x: -1, y: -1, z: -1), new Vector3(x: 1, y: -1, z: -1), new Vector3(x: 1, y: 1, z: -1),
		new Vector3(x: -1, y: 1, z: -1), // near plane
		new Vector3(x: -1, y: -1, z: 1), new Vector3(x: 1, y: -1, z: 1), new Vector3(x: 1, y: 1, z: 1),
		new Vector3(x: -1, y: 1, z: 1) // far flane
	};

	protected Camera(float worldWidth, float worldHeight)
	{
		WorldSize = new Vector2(worldWidth, worldHeight);
	}

	public Vector3 Position { get; set; } = Vector3.Zero;
	public Quaternion Rotation { get; set; } = Quaternion.Identity;

	public Vector3 Forward
	{
		get => Vector3.Transform(-Vector3.UnitZ, Rotation);
		set
		{
			var (pitch, yaw, _) = MathUtils.QuaternionLookRotation(value, Vector3.UnitY).ToEulerAngles();
			Pitch = pitch;
			Yaw = yaw;
		}
	}

	public Vector3 Up
	{
		get => Vector3.Transform(Vector3.UnitY, Rotation);
		set
		{
			var (pitch, yaw, _) = MathUtils.QuaternionFromToRotation(Vector3.UnitY, value).ToEulerAngles();
			Pitch = pitch;
			Yaw = yaw;
		}
	}

	public Vector3 Right => Vector3.Cross(Forward, Up);

	public float Pitch
	{
		get => Rotation.ToEulerAngles().X;
		set
		{
			var angles = Rotation.ToEulerAngles();
			Rotation = Quaternion.FromEulerAngles(value, angles.Y, angles.Z);
		}
	}

	public float Yaw
	{
		get => Rotation.ToEulerAngles().Y;
		set
		{
			var angles = Rotation.ToEulerAngles();
			Rotation = Quaternion.FromEulerAngles(angles.X, value, angles.Z);
		}
	}

	public float Roll
	{
		get => Rotation.ToEulerAngles().Z;
		set
		{
			var angles = Rotation.ToEulerAngles();
			Rotation = Quaternion.FromEulerAngles(angles.X, angles.Y, value);
		}
	}

	public Viewport Viewport { get; private set; }
	public Vector2 WindowSize { get; private set; }
	public Vector2 WorldSize { get; set; }

	public float NearClippingPlane { get; set; } = 1;
	public float FarClippingPlane { get; set; } = 100;

	public Matrix4 Combined => Projection * View;

	public abstract Matrix4 Projection { get; }
	public abstract Matrix4 View { get; }
	
	public abstract Frustum Frustum { get; }

	protected abstract Viewport UpdateViewport(int windowWidth, int windowHeight);

	[Obsolete($"Use {nameof(UpdateWindowSize)} instead")]
	public void Update(int newWidth, int newHeight)
	{
		UpdateWindowSize(newWidth, newHeight);
	}

	/// <summary>
	///     Adjusts this camera's viewport to match the new window width and height. Important to do after the window is
	///     resized.
	/// </summary>
	/// <param name="newWindowWidth">the new window width</param>
	/// <param name="newWindowHeight">the new window height</param>
	public void UpdateWindowSize(int newWindowWidth, int newWindowHeight)
	{
		WindowSize = new Vector2(newWindowWidth, newWindowHeight);
		Viewport = UpdateViewport(newWindowWidth, newWindowHeight);
		Viewport.Apply();
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