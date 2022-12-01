using JXS.Graphics.Core;
using JXS.Graphics.Text;
using OpenTK.Mathematics;

namespace JXS.Gui;

public interface IGraphicsProvider
{
	void Begin();
	void End();

	void DrawRect(Box2 bounds, Color4<Rgba> color);
	void DrawImage(Box2 bounds, Texture2D texture);

	void DrawText(Font font, int size, string text, Vector2 position, Color4<Rgba> color, float maxTextWidth, bool log = false);

	Vector2 MeasureText(Font font, int size, string text);
}