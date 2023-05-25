using JetBrains.Annotations;
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
		new Vector3(x: -1, y: 1, z: 1) // far plane
	};

	private Vector2 windowSize;

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

	/// <summary>
	///     The size (in pixels) of the OpenGL window. If the window size changes the new size should be assigned here
	///     so the camera can properly calculate the new viewport.
	/// </summary>
	public Vector2 WindowSize
	{
		get => windowSize;
		set
		{
			windowSize = value;
			ApplyViewport();
		}
	}

	/// <summary>
	///     The size (in "world units") of the virtual world that the camera is displaying.
	/// </summary>
	public Vector2 WorldSize { get; set; }

	public float NearClippingPlane { get; set; } = 1;
	public float FarClippingPlane { get; set; } = 100;

	public Matrix4 Combined => Projection * View;

	public abstract Matrix4 Projection { get; }
	public abstract Matrix4 View { get; }

	public abstract Frustum Frustum { get; }

	protected abstract Viewport UpdateViewport(int windowWidth, int windowHeight);

	[Obsolete($"Set {nameof(WindowSize)} directly instead")]
	public void Update(int newWidth, int newHeight) =>
		WindowSize = new Vector2(newWidth, newHeight);

	/// <summary>
	///     Adjusts this camera's viewport to match the new window width and height. Important to do after the window is
	///     resized.
	/// </summary>
	/// <param name="newWindowWidth">the new window width</param>
	/// <param name="newWindowHeight">the new window height</param>
	[Obsolete($"Set {nameof(WindowSize)} directly instead")]
	public void UpdateWindowSize(int newWindowWidth, int newWindowHeight) =>
		WindowSize = new Vector2(newWindowWidth, newWindowHeight);

	public void ApplyViewport()
	{
		Viewport = UpdateViewport((int)WindowSize.X, (int)WindowSize.Y);
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

	[Pure]
	public Vector3 UnProject(Vector3 screenCoords)
	{
		var (viewportX, viewportY, viewportWidth, viewportHeight) = Viewport;
		var x = screenCoords.X - viewportX;
		var y = WindowSize.Y - screenCoords.Y - viewportY;
		screenCoords.X = x * 2 / viewportWidth - 1;
		screenCoords.Y = y * 2 / viewportHeight - 1;

		var vec4 = new Vector4(screenCoords, w: 1);
		vec4 *= Matrix4.Invert(Projection);
		vec4 *= Matrix4.Invert(View);

		if (vec4.W is > float.Epsilon or < -float.Epsilon)
		{
			var w = vec4.W;
			vec4 /= vec4.W;
			vec4.W = w;
		}

		return vec4.Xyz;
		// return Vector3.Unproject(screenCoords, viewportX, viewportY, viewportWidth, viewportHeight, NearClippingPlane,
		// 	FarClippingPlane, Matrix4.Invert(Combined * Matrix4.CreateTranslation(Position)));
	}

	[Pure]
	public Vector2 UnProject(Vector2 screenCoords) => UnProject(new Vector3(screenCoords)).Xy;

	// public Ray ScreenPointToRay(Vector2 point)
	// {
	// 	var nearPoint = latestViewport.Unproject(point.AsXY(latestViewport.MinDepth), Projection, View, Matrix4.Identity);
	// 	var farPoint = latestViewport.Unproject(point.AsXY(latestViewport.MaxDepth), Projection, View, Matrix4.Identity);
	// 	var direction = (farPoint - nearPoint).Normalized();
	// 	return new Ray(nearPoint, direction);
	// }
}