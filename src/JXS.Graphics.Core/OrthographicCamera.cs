using OpenTK.Graphics.OpenGL;
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

	public override Matrix4 View => Matrix4.LookAt(Position, Position + Forward, Up);

	protected override void UpdateViewport(int screenWidth, int screenHeight)
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
		GL.Viewport(
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