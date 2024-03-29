﻿using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.G2D;
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

	void DrawImage(Box2 bounds, Texture2D texture,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default);

	void DrawNinePatch(NinePatch ninePatch, Box2 bounds, Color4<Rgba> color = default);

	void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color);
	void DrawText(Font font, string text, int fontSize, Vector2 position, Color4<Rgba> color);
}