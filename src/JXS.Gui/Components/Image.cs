using Facebook.Yoga;
using JXS.Graphics.Core;

namespace JXS.Gui.Components;

public class Image : Component
{
    public Image(Texture2D? texture, Style? style, string? id, IInputProvider inputProvider) : base(style, id, inputProvider)
    {
        Texture = texture;
    }

    public Texture2D? Texture { get; set; }

    public override void Draw(IGraphicsProvider graphicsProvider)
    {
        base.Draw(graphicsProvider);
        if (Texture is null) return;

        if (Style.Overflow == YogaOverflow.Hidden)
        {
            var scissor = CalculatedBounds.Floor();
            graphicsProvider.AddScissor(scissor);
            graphicsProvider.DrawImage(Texture, CalculatedBounds);
            graphicsProvider.RemoveScissor(scissor);
        }
        else
        {
            graphicsProvider.DrawImage(Texture, CalculatedBounds);
        }
    }
}