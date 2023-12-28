using CodeFirst.Utils.Math;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public class OrthographicCamera : Camera
{
	public OrthographicCamera(float worldWidth, float worldHeight) : base(worldWidth, worldHeight)
	{
	}

	public override Matrix4 Projection =>
		Matrix4.CreateOrthographic(WorldSize.X, WorldSize.Y, NearClippingPlane, FarClippingPlane);

	public override Matrix4 View => Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);

	public ViewportScaling Scaling { get; set; } = ViewportScaling.Stretch;

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
		var viewportSize = Scale(WorldSize.X, WorldSize.Y, screenWidth, screenHeight);
		var (viewportWidth, viewportHeight) = ApplyViewportScaling(
			screenWidth,
			screenHeight,
			WorldSize.X,
			WorldSize.Y,
			MathF.Round(viewportSize.X),
			MathF.Round(viewportSize.Y)
		);
		
		return new Viewport(
			(int)(screenWidth - viewportWidth) / 2,
			(int)(screenHeight - viewportHeight) / 2,
			(int)viewportWidth,
			(int)viewportHeight
		);
	}

	private (float, float) ApplyViewportScaling(
		int screenWidth, int screenHeight,
		float worldWidth, float worldHeight,
		float viewportWidth, float viewportHeight)
		=> Scaling switch
		{
			ViewportScaling.Fit => (viewportWidth, viewportHeight),
			_ => ApplyStretchViewportScaling(
				screenWidth, screenHeight, 
				worldWidth, worldHeight, 
				viewportWidth, viewportHeight)
		};

	private static (float, float) ApplyStretchViewportScaling(
		int screenWidth, int screenHeight, 
		float worldWidth, float worldHeight, 
		float viewportWidth, float viewportHeight)
	{
		if (viewportWidth < screenWidth)
		{
			var toViewportSpace = viewportHeight / worldHeight;
			var toWorldSpace = worldHeight / viewportHeight;
			var lengthen = (screenWidth - viewportWidth) * toWorldSpace;
			viewportWidth += MathF.Round(lengthen * toViewportSpace);
		}
		else if (viewportHeight < screenHeight)
		{
			var toViewportSpace = viewportWidth / worldWidth;
			var toWorldSpace = worldWidth / viewportWidth;
			var lengthen = (screenHeight - viewportHeight) * toWorldSpace;
			viewportHeight += MathF.Round(lengthen * toViewportSpace);
		}

		return (viewportWidth, viewportHeight);
	}

	private static Vector2 Scale(float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
	{
		var targetRatio = targetHeight / targetWidth;
		var sourceRatio = sourceHeight / sourceWidth;
		var scale = targetRatio > sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
		return new Vector2(sourceWidth, sourceHeight) * scale;
	}
}

public enum ViewportScaling
{
	Fit,
	Stretch
}