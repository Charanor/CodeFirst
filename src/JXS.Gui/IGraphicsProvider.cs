using JXS.Graphics.Core;
using JXS.Graphics.Text;
using JXS.Graphics.Text.Layout;
using OpenTK.Mathematics;

namespace JXS.Gui;

public interface IGraphicsProvider
{
	void Begin();
	void End();

	void BeginOverflow();
	void EndOverflow();

	void DrawRect(Box2 bounds, Color4<Rgba> color);
	void DrawImage(Box2 bounds, Texture2D texture);

	void DrawText(Font font, string text, int size, Vector2 position, Color4<Rgba> color, float maxTextWidth);
	void DrawText(Font font, IEnumerable<TextRow> rows, int size, Vector2 position, Color4<Rgba> color);
	void DrawText(Font font, TextRow row, int size, Vector2 position, Color4<Rgba> color);
}