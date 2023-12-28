using CodeFirst.Graphics.Core.Utils;
using CodeFirst.Utils.Math;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public abstract class Camera
{
	private const float REALLY_SMALL = 0.0000001f;

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
			var (pitch, yaw, _) = MathUtils.QuaternionLookRotation(Vector3.Normalize(value), Vector3.UnitY)
				.ToEulerAngles();
			Pitch = pitch;
			Yaw = yaw;
		}
	}

	public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
	public Vector3 Up => Vector3.Normalize(Vector3.Cross(Forward, Right));

	// public Vector3 Up
	// {
	// 	get => Vector3.Transform(Vector3.UnitY, Rotation);
	// 	set
	// 	{
	// 		var (pitch, yaw, _) = MathUtils.QuaternionFromToRotation(Vector3.UnitY, Vector3.Normalize(value))
	// 			.ToEulerAngles();
	// 		Pitch = pitch;
	// 		Yaw = yaw;
	// 	}
	// }

	public float Pitch
	{
		get => Rotation.X;
		set => Rotation = Quaternion.FromEulerAngles(value, Yaw, Roll);
	}

	public float Yaw
	{
		get => Rotation.Y;
		set => Rotation = Quaternion.FromEulerAngles(Pitch, value, Roll);
	}

	public float Roll
	{
		get => Rotation.Z;
		set => Rotation = Quaternion.FromEulerAngles(Pitch, Yaw, value);
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

	public Box2 OrthographicBounds => new(Position.Xy - WorldSize / 2f, Position.Xy + WorldSize / 2f);

	// public Frustum Frustum
	// {
	// get
	// {
	// 	// var View = this.View * Matrix4.CreateTranslation(Position);
	// 	var clipMatrix = new float[16];
	//
	// 	clipMatrix[0] = View.M11 * Projection.M11 + View.M12 * Projection.M21 + View.M13 * Projection.M31 +
	// 	                View.M14 * Projection.M41;
	// 	clipMatrix[1] = View.M11 * Projection.M12 + View.M12 * Projection.M22 + View.M13 * Projection.M32 +
	// 	                View.M14 * Projection.M42;
	// 	clipMatrix[2] = View.M11 * Projection.M13 + View.M12 * Projection.M23 + View.M13 * Projection.M33 +
	// 	                View.M14 * Projection.M43;
	// 	clipMatrix[3] = View.M11 * Projection.M14 + View.M12 * Projection.M24 + View.M13 * Projection.M34 +
	// 	                View.M14 * Projection.M44;
	//
	// 	clipMatrix[4] = View.M21 * Projection.M11 + View.M22 * Projection.M21 + View.M23 * Projection.M31 +
	// 	                View.M24 * Projection.M41;
	// 	clipMatrix[5] = View.M21 * Projection.M12 + View.M22 * Projection.M22 + View.M23 * Projection.M32 +
	// 	                View.M24 * Projection.M42;
	// 	clipMatrix[6] = View.M21 * Projection.M13 + View.M22 * Projection.M23 + View.M23 * Projection.M33 +
	// 	                View.M24 * Projection.M43;
	// 	clipMatrix[7] = View.M21 * Projection.M14 + View.M22 * Projection.M24 + View.M23 * Projection.M34 +
	// 	                View.M24 * Projection.M44;
	//
	// 	clipMatrix[8] = View.M31 * Projection.M11 + View.M32 * Projection.M21 + View.M33 * Projection.M31 +
	// 	                View.M34 * Projection.M41;
	// 	clipMatrix[9] = View.M31 * Projection.M12 + View.M32 * Projection.M22 + View.M33 * Projection.M32 +
	// 	                View.M34 * Projection.M42;
	// 	clipMatrix[10] = View.M31 * Projection.M13 + View.M32 * Projection.M23 + View.M33 * Projection.M33 +
	// 	                 View.M34 * Projection.M43;
	// 	clipMatrix[11] = View.M31 * Projection.M14 + View.M32 * Projection.M24 + View.M33 * Projection.M34 +
	// 	                 View.M34 * Projection.M44;
	//
	// 	clipMatrix[12] = View.M41 * Projection.M11 + View.M42 * Projection.M21 + View.M43 * Projection.M31 +
	// 	                 View.M44 * Projection.M41;
	// 	clipMatrix[13] = View.M41 * Projection.M12 + View.M42 * Projection.M22 + View.M43 * Projection.M32 +
	// 	                 View.M44 * Projection.M42;
	// 	clipMatrix[14] = View.M41 * Projection.M13 + View.M42 * Projection.M23 + View.M43 * Projection.M33 +
	// 	                 View.M44 * Projection.M43;
	// 	clipMatrix[15] = View.M41 * Projection.M14 + View.M42 * Projection.M24 + View.M43 * Projection.M34 +
	// 	                 View.M44 * Projection.M44;
	//
	// 	var nearVector = new Vector4(clipMatrix[0], clipMatrix[1], clipMatrix[2], clipMatrix[3]).Normalized();
	// 	var near = new Plane(nearVector.Xyz, nearVector.W);
	// 	var farVector = new Vector4(clipMatrix[4], clipMatrix[5], clipMatrix[6], clipMatrix[7]).Normalized();
	// 	var far = new Plane(farVector.Xyz, farVector.W);
	// 	var leftVector = new Vector4(clipMatrix[8], clipMatrix[9], clipMatrix[10], clipMatrix[11]).Normalized();
	// 	var left = new Plane(leftVector.Xyz, leftVector.W);
	// 	var rightVector = new Vector4(clipMatrix[12], clipMatrix[13], clipMatrix[14], clipMatrix[15]).Normalized();
	// 	var right = new Plane(rightVector.Xyz, rightVector.W);
	//
	// 	var topVector = new Vector4(clipMatrix[4] - clipMatrix[1], clipMatrix[5] - clipMatrix[9],
	// 		clipMatrix[6] - clipMatrix[13], clipMatrix[7] - clipMatrix[15]).Normalized();
	// 	var top = new Plane(topVector.Xyz, topVector.W);
	// 	var bottomVector = new Vector4(clipMatrix[4] + clipMatrix[1], clipMatrix[5] + clipMatrix[9],
	// 		clipMatrix[6] + clipMatrix[13], clipMatrix[7] + clipMatrix[15]).Normalized();
	// 	var bottom = new Plane(bottomVector.Xyz, bottomVector.W);
	//
	// 	return new Frustum(near, far, left, right, top, bottom);
	// }
	// }

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
			// Up = -Forward;
		}
		else if (MathF.Abs(dot + 1) < REALLY_SMALL)
		{
			// Up = Forward;
		}

		Forward = newForward;
		NormalizeUp();
	}

	public void NormalizeUp()
	{
		// Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
	}

	public void Rotate(Vector3 axis, float angleRadians)
	{
		Forward = Vector3.Transform(Forward, Quaternion.FromAxisAngle(axis, angleRadians));
		// Up = Vector3.Transform(Up, Quaternion.FromAxisAngle(axis, angleRadians)).Normalized();
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