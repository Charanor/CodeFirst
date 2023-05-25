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

	void DrawRect(Box2 bounds, Color4<Rgba> color,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default);

	void DrawImage(Box2 bounds, Texture2D texture);

	void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color);
}