using Facebook.Yoga;
using JXS.Graphics.Core;

namespace JXS.Gui.Components;

public class Image : Component
{
	public Image(string? id = default, Texture2D? texture = default, Style? style = default) : base(id, style)
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

		if (Style.Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			{
				graphicsProvider.DrawImage(CalculatedBounds, Texture);
			}
			graphicsProvider.EndOverflow();
		}
		else
		{
			graphicsProvider.DrawImage(CalculatedBounds, Texture);
		}
	}
}