using JXS.Utils.Math;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public class OrthographicCamera : Camera
{
	private readonly float baseWorldWidth;
	private readonly float baseWorldHeight;

	public OrthographicCamera(float worldWidth, float worldHeight) : base(worldWidth, worldHeight)
	{
		baseWorldWidth = worldWidth;
		baseWorldHeight = worldHeight;
	}

	public override Matrix4 Projection =>
		Matrix4.CreateOrthographic(WorldSize.X, WorldSize.Y, NearClippingPlane, FarClippingPlane);

	public override Matrix4 View => Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);

	public override Frustum Frustum
	{
		get
		{
			var aspectRatio = WorldSize.X / WorldSize.Y;
			var nearHeight = 2 * MathF.Tan(0) * NearClippingPlane;
			var nearWidth = nearHeight * aspectRatio;
			var farHeight = 2 * MathF.Tan(0) * FarClippingPlane;
			var farWidth = farHeight * aspectRatio;
	
			var nearCenter = Position + Forward * NearClippingPlane;
			var farCenter = Position + Forward * FarClippingPlane;
	
			var nearBottomLeft = nearCenter - Up * (nearHeight / 2.0f) - Right * (nearWidth / 2.0f);
			var nearBottomRight = nearCenter - Up * (nearHeight / 2.0f) + Right * (nearWidth / 2.0f);
			var nearTopLeft = nearCenter + Up * (nearHeight / 2.0f) - Right * (nearWidth / 2.0f);
			var nearTopRight = nearCenter + Up * (nearHeight / 2.0f) + Right * (nearWidth / 2.0f);
	
			var farBottomLeft = farCenter - Up * (farHeight / 2.0f) - Right * (farWidth / 2.0f);
			var farBottomRight = farCenter - Up * (farHeight / 2.0f) + Right * (farWidth / 2.0f);
			var farTopLeft = farCenter + Up * (farHeight / 2.0f) - Right * (farWidth / 2.0f);
			var farTopRight = farCenter + Up * (farHeight / 2.0f) + Right * (farWidth / 2.0f);
	
			return new Frustum(
				new Plane(nearBottomLeft, nearBottomRight, nearTopLeft), // Near
				new Plane(farTopLeft, farTopRight, farBottomLeft), // Far
				new Plane(nearBottomLeft, nearTopLeft, farBottomLeft), // Left
				new Plane(farTopRight, nearTopRight, farBottomRight), // Right
				new Plane(farTopLeft, nearTopLeft, farTopRight), // Top
				new Plane(nearBottomLeft, farBottomLeft, nearBottomRight) // Bottom
			);
		}
	}

	protected override Viewport UpdateViewport(int screenWidth, int screenHeight)
	{
		var newWorldWidth = baseWorldWidth;
		var newWorldHeight = baseWorldHeight;
		var scaled = Scale(newWorldWidth, newWorldHeight, screenWidth, screenHeight);
		var viewportWidth = MathF.Round(scaled.X);
		var viewportHeight = MathF.Round(scaled.Y);

		if (viewportWidth < screenWidth)
		{
			var toViewportSpace = viewportHeight / newWorldHeight;
			var toWorldSpace = newWorldHeight / viewportHeight;
			var lengthen = (screenWidth - viewportWidth) * toWorldSpace;
			newWorldWidth += lengthen;
			viewportWidth += MathF.Round(lengthen * toViewportSpace);
		}
		else if (viewportHeight < screenHeight)
		{
			var toViewportSpace = viewportWidth / newWorldWidth;
			var toWorldSpace = newWorldWidth / viewportWidth;
			var lengthen = (screenHeight - viewportHeight) * toWorldSpace;
			newWorldHeight += lengthen;
			viewportHeight += MathF.Round(lengthen * toViewportSpace);
		}

		WorldSize = new Vector2(newWorldWidth, newWorldHeight);
		return new Viewport(
			(int)(screenWidth - viewportWidth) / 2,
			(int)(screenHeight - viewportHeight) / 2,
			(int)viewportWidth,
			(int)viewportHeight
		);
	}

	private static Vector2 Scale(float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
	{
		var targetRatio = targetHeight / targetWidth;
		var sourceRatio = sourceHeight / sourceWidth;
		var scale = targetRatio > sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
		return new Vector2(sourceWidth, sourceHeight) * scale;
	}
}