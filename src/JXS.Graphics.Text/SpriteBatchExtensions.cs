using System.ComponentModel;
using JXS.Graphics.Renderer2D;

namespace JXS.Graphics.Text;

public enum ScreenspaceTextDrawMode
{
	Normal,
	Immediate
}

public static class SpriteBatchExtensions
{
	public static void Draw(this SpriteBatch spriteBatch, ScreenspaceText text,
		ScreenspaceTextDrawMode drawMode = ScreenspaceTextDrawMode.Normal)
	{
		switch (drawMode)
		{
			case ScreenspaceTextDrawMode.Normal:
				text.Draw(spriteBatch);
				break;
			case ScreenspaceTextDrawMode.Immediate:
				text.DrawImmediate(spriteBatch);
				break;
			default:
				throw new InvalidEnumArgumentException(nameof(drawMode), (int)drawMode,
					typeof(ScreenspaceTextDrawMode));
		}
	}
}