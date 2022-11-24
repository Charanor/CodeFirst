using JXS.Graphics.Core;
using OpenTK.Mathematics;

namespace JXS.Gui;

public interface IGraphicsProvider
{
	void Begin();
	void End();

	void AddScissor(Box2i bounds);
	void RemoveScissor(Box2i bounds);

	void DrawRect(Box2 bounds, Color4<Rgba> color);
	void DrawImage(Texture2D texture, Box2 bounds);

	void DrawText(int fontSize, string text, Vector2 position, Color4<Rgba> color, float maxTextWidth,
		bool log = false);

	Vector2 MeasureText(int fontSize, string text);
}