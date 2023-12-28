using CodeFirst.Graphics.Core;
using Facebook.Yoga;

namespace CodeFirst.Gui.Components;

public class Image : Frame
{
	public Image(Texture2D? texture = default)
	{
		Texture = texture;
	}

	public Texture2D? Texture { get; set; }

	public override void Draw(IGraphicsProvider graphicsProvider)
	{
		base.Draw(graphicsProvider);
		if (Texture is null)
		{
			return;
		}

		if (Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			{
				graphicsProvider.DrawImage(TransformedBounds, Texture);
			}
			graphicsProvider.EndOverflow();
		}
		else
		{
			graphicsProvider.DrawImage(TransformedBounds, Texture);
		}
	}
}