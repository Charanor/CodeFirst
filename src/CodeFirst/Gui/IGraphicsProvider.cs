using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.Text;
using CodeFirst.Graphics.Text.Layout;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public interface IGraphicsProvider
{
	void Begin(bool centerCamera = true);
	void End();

	void BeginOverflow();
	void EndOverflow();

	void DrawRect(Box2 bounds, Color4<Rgba> color,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default);

	void DrawWindow(
		Box2 bounds,
		float borderSize,
		Texture2D? topLeft,
		Texture2D? topEdge,
		Texture2D? topRight,
		Texture2D? bottomLeft,
		Texture2D? bottomEdge,
		Texture2D? bottomRight,
		Texture2D? leftEdge,
		Texture2D? rightEdge,
		Texture2D? fill,
		bool tileFill = false,
		bool flipEdges = false,
		bool flipCorners = false
	);

	void DrawWindow(
		Box2 bounds,
		float borderSize,
		Texture2D? topLeft,
		Texture2D? topRight,
		Texture2D? bottomLeft,
		Texture2D? bottomRight,
		Texture2D? edges,
		Texture2D? fill,
		bool tileFill = false,
		bool flipEdges = false,
		bool flipCorners = false
	);

	void DrawWindow(
		Box2 bounds,
		float borderSize,
		Texture2D? corners,
		Texture2D? edges,
		Texture2D? fill,
		bool tileFill = false,
		bool flipEdges = false,
		bool flipCorners = false
	);

	void DrawImage(Box2 bounds, Texture2D texture,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default,
		bool flipX = false,
		bool flipY = false,
		bool flipAxis = false);

	void DrawNinePatch(NinePatch ninePatch, Box2 bounds);

	void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color);
	void DrawText(Font font, string text, int fontSize, Vector2 position, Color4<Rgba> color);
}