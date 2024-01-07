using System.Diagnostics;
using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.G2D;
using CodeFirst.Graphics.Generated;
using CodeFirst.Graphics.Text;
using CodeFirst.Graphics.Text.Layout;
using OpenTK.Mathematics;
using StencilOp = OpenTK.Graphics.OpenGL.StencilOp;

namespace CodeFirst.Gui;

public sealed class GLGraphicsProvider : IGraphicsProvider, IDisposable
{
	private readonly SpriteBatch spriteBatch;
	private readonly BasicGraphicsShader basicGraphicsShader;

	private int overflowLayer;

	public GLGraphicsProvider(SpriteBatch spriteBatch, Camera? camera = null)
	{
		this.spriteBatch = spriteBatch;
		basicGraphicsShader = new BasicGraphicsShader();
		Camera = camera;
		overflowLayer = 0;
	}

	public Camera? Camera
	{
		get => spriteBatch.Camera;
		set => spriteBatch.Camera = value;
	}

	public void Dispose()
	{
		basicGraphicsShader.Dispose();
	}

	public void Begin(bool centerCamera = true)
	{
		if (Camera == null)
		{
			return;
		}

		if (centerCamera)
		{
			Camera.Position = new Vector3(Camera.WorldSize.X / 2, Camera.WorldSize.Y / 2, z: 0);
		}

		spriteBatch.Camera = Camera;
		spriteBatch.Shader = basicGraphicsShader;
		spriteBatch.Begin();

		Enable(EnableCap.Blend);
		BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		Clear(ClearBufferMask.StencilBufferBit);
		Enable(EnableCap.StencilTest);
		StencilFunc(StencilFunction.Always, reference: 1, mask: 0xff);
		StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
		StencilMask(0xff);

		Disable(EnableCap.DepthTest);
	}

	public void End()
	{
		Debug.Assert(overflowLayer == 0);
		spriteBatch.End();

		Clear(ClearBufferMask.StencilBufferBit);
		Disable(EnableCap.Blend);
		Disable(EnableCap.StencilTest);
		Disable(EnableCap.DepthTest);
	}

	public void BeginOverflow()
	{
		overflowLayer += 1;
		StencilFunc(StencilFunction.Lequal, overflowLayer, mask: 0xff);
		StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
	}

	public void EndOverflow()
	{
		overflowLayer -= 1;
		StencilFunc(StencilFunction.Lequal, overflowLayer, mask: 0xff);
	}

	public void DrawImage(Box2 bounds, Texture2D texture,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default)
	{
		if (Camera == null)
		{
			return;
		}

		var prevShader = spriteBatch.Shader;
		spriteBatch.Shader = basicGraphicsShader;
		{
			basicGraphicsShader.Size = bounds.Size;
			basicGraphicsShader.BorderTopLeftRadius = borderTopLeftRadius;
			basicGraphicsShader.BorderTopRightRadius = borderTopRightRadius;
			basicGraphicsShader.BorderBottomLeftRadius = borderBottomLeftRadius;
			basicGraphicsShader.BorderBottomRightRadius = borderBottomRightRadius;

			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			spriteBatch.Draw(texture, (bounds.X, convertedY), bounds.Size / 2f, bounds.Size, color: Color4.White);
		}
		spriteBatch.Shader = prevShader;
	}

	public void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		if (Camera == null)
		{
			return;
		}

		font.Draw(spriteBatch, row, fontSize, position, color);
	}

	public void DrawText(Font font, string text, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		if (Camera == null)
		{
			return;
		}

		font.Draw(spriteBatch, text, fontSize, position, color);
	}

	public void DrawRect(Box2 bounds, Color4<Rgba> color,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default)
	{
		if (Camera == null)
		{
			return;
		}

		var prevShader = spriteBatch.Shader;
		spriteBatch.Shader = basicGraphicsShader;
		{
			basicGraphicsShader.Size = bounds.Size;
			basicGraphicsShader.BorderTopLeftRadius = borderTopLeftRadius;
			basicGraphicsShader.BorderTopRightRadius = borderTopRightRadius;
			basicGraphicsShader.BorderBottomLeftRadius = borderBottomLeftRadius;
			basicGraphicsShader.BorderBottomRightRadius = borderBottomRightRadius;

			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			spriteBatch.Draw(color, (bounds.X, convertedY), Vector2.Zero, bounds.Size);
		}
		spriteBatch.Shader = prevShader;
	}

	public void DrawNinePatch(NinePatch ninePatch, Box2 bounds, Color4<Rgba> color = default)
	{
		if (Camera == null)
		{
			return;
		}
		
		spriteBatch.Draw(ninePatch, bounds, color);

		// End();
		// var oldZ = Camera.Position.Z;
		// Camera.Position = Camera.Position with
		// {
		// 	Z = 0
		// };
		// spriteBatch.Camera = Camera;
		// spriteBatch.Begin();
		// {
		// 	var vertices = ninePatch.GetVertices(bounds, color).Select(vert => vert with
		// 	{
		// 		Position = vert.Position with
		// 		{
		// 			Y = Camera.WorldSize.Y - vert.Position.Y
		// 		}
		// 	}).ToArray();
		// 	spriteBatch.Draw((Texture2D)ninePatch.Texture, vertices, offset: 0, vertices.Length);
		// }
		// spriteBatch.End();
		// Camera.Position = Camera.Position with
		// {
		// 	Z = oldZ
		// };
		// Begin();
	}
}