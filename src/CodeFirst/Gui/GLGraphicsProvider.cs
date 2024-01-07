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

		var hasBorderRadius = borderTopLeftRadius > 0 || borderTopRightRadius > 0 || borderBottomLeftRadius > 0 ||
		                      borderBottomRightRadius > 0;

		using (spriteBatch.SetTemporaryShader(hasBorderRadius ? basicGraphicsShader : null))
		{
			if (hasBorderRadius)
			{
				basicGraphicsShader.Size = bounds.Size;
				basicGraphicsShader.BorderTopLeftRadius = borderTopLeftRadius;
				basicGraphicsShader.BorderTopRightRadius = borderTopRightRadius;
				basicGraphicsShader.BorderBottomLeftRadius = borderBottomLeftRadius;
				basicGraphicsShader.BorderBottomRightRadius = borderBottomRightRadius;
			}

			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			spriteBatch.Draw(texture, (bounds.X, convertedY), bounds.Size / 2f, bounds.Size, Color4.White);
		}
	}

	public void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		font.Draw(spriteBatch, row, fontSize, position, color);
	}

	public void DrawText(Font font, string text, int fontSize, Vector2 position, Color4<Rgba> color)
	{
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

		using (spriteBatch.SetTemporaryShader(basicGraphicsShader))
		{
			basicGraphicsShader.Size = bounds.Size;
			basicGraphicsShader.BorderTopLeftRadius = borderTopLeftRadius;
			basicGraphicsShader.BorderTopRightRadius = borderTopRightRadius;
			basicGraphicsShader.BorderBottomLeftRadius = borderBottomLeftRadius;
			basicGraphicsShader.BorderBottomRightRadius = borderBottomRightRadius;

			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			spriteBatch.Draw(color, (bounds.X, convertedY), Vector2.Zero, bounds.Size);
		}
	}

	public void DrawNinePatch(NinePatch ninePatch, Box2 bounds, Color4<Rgba> color)
	{
		spriteBatch.Draw(ninePatch, bounds, color);
	}
}