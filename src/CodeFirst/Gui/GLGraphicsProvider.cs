using System.Diagnostics;
using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.Core.Utils;
using CodeFirst.Graphics.G2D;
using CodeFirst.Graphics.Generated;
using CodeFirst.Graphics.Text;
using CodeFirst.Graphics.Text.Layout;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using StencilOp = OpenTK.Graphics.OpenGL.StencilOp;

namespace CodeFirst.Gui;

public class GLGraphicsProvider : IGraphicsProvider, IDisposable
{
	private static readonly Vertex[] QuadVertices =
	{
		new(new Vector2(x: 0, y: 0)),
		new(new Vector2(x: 1, y: 0)),
		new(new Vector2(x: 1, y: 1)),
		new(new Vector2(x: 0, y: 1))
	};

	private static readonly uint[] QuadIndices =
	{
		0u, 1u, 3u,
		1u, 2u, 3u
	};

	private readonly Buffer<Vertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;
	private readonly BasicGraphicsShader shader;

	private readonly SpriteBatch spriteBatch;

	private int overflowLayer;

	private ShaderProgram? activeShader;

	public GLGraphicsProvider(SpriteBatch spriteBatch, Camera? camera = null)
	{
		this.spriteBatch = spriteBatch;
		Camera = camera;
		vertexBuffer = new Buffer<Vertex>(QuadVertices, VertexBufferObjectUsage.StaticRead);
		indexBuffer = new Buffer<uint>(QuadIndices, VertexBufferObjectUsage.StaticRead);
		vertexArray = VertexArray.CreateForVertexInfo(Vertex.VertexInfo, vertexBuffer, indexBuffer);
		shader = new BasicGraphicsShader();
		overflowLayer = 0;
	}

	public Camera? Camera { get; set; }

	private ShaderProgram? ActiveShader
	{
		get => activeShader;
		set
		{
			if (activeShader == value)
			{
				return;
			}

			activeShader?.Unbind();
			activeShader = value;
			activeShader?.Bind();
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		vertexBuffer.Dispose();
		indexBuffer.Dispose();
		vertexArray.Dispose();
	}

	public void Begin(bool centerCamera = true)
	{
		if (Camera == null)
		{
			return;
		}

		Enable(EnableCap.Blend);
		BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		Clear(ClearBufferMask.StencilBufferBit);
		Enable(EnableCap.StencilTest);
		StencilFunc(StencilFunction.Always, reference: 1, mask: 0xff);
		StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
		StencilMask(0xff);

		Disable(EnableCap.DepthTest);

		if (centerCamera)
		{
			Camera.Position = new Vector3(Camera.WorldSize.X / 2, Camera.WorldSize.Y / 2, z: 1);
		}

		shader.ProjectionMatrix = Camera.Projection;
		shader.ViewMatrix = Camera.View;
		ActiveShader = shader;

		vertexArray.Bind();
	}

	public void End()
	{
		Debug.Assert(overflowLayer == 0);
		vertexArray.Unbind();
		ActiveShader = null;

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

	public void DrawWindow(Box2 bounds,
		float borderSize, Texture2D? corners, Texture2D? edges, Texture2D? fill, bool tileFill = false,
		bool flipEdges = false, bool flipCorners = false)
	{
		DrawWindow(bounds, borderSize, corners, corners, corners, corners, edges, fill, tileFill, flipEdges,
			flipCorners);
	}

	public void DrawWindow(Box2 bounds,
		float borderSize, Texture2D? topLeft, Texture2D? topRight, Texture2D? bottomLeft,
		Texture2D? bottomRight,
		Texture2D? edges, Texture2D? fill, bool tileFill = false,
		bool flipEdges = false, bool flipCorners = false)
	{
		DrawWindow(bounds, borderSize, topLeft, edges, topRight, bottomLeft, edges, bottomRight, edges, edges, fill,
			tileFill, flipEdges, flipCorners);
	}

	public void DrawWindow(Box2 bounds,
		float borderSize, Texture2D? topLeft, Texture2D? topEdge, Texture2D? topRight,
		Texture2D? bottomLeft,
		Texture2D? bottomEdge, Texture2D? bottomRight, Texture2D? leftEdge, Texture2D? rightEdge, Texture2D? fill,
		bool tileFill = false, bool flipEdges = false, bool flipCorners = false)
	{
		const float padding = 0;

		// Draw corners
		var offset = Vector2.Zero;
		var cornerSize = new Vector2(borderSize);
		if (topLeft != null)
		{
			DrawImage(Box2.FromSize(bounds.Location + offset, cornerSize), topLeft);
		}

		offset = new Vector2(x: 0, bounds.Height - borderSize);
		if (bottomLeft != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset, cornerSize),
				bottomLeft,
				flipY: flipCorners);
		}

		offset = new Vector2(bounds.Width - borderSize, y: 0);
		if (topRight != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset, cornerSize),
				topRight,
				flipX: flipCorners);
		}

		offset = new Vector2(bounds.Width - borderSize, bounds.Height - borderSize);
		if (bottomRight != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset, cornerSize),
				bottomRight,
				flipX: flipCorners,
				flipY: flipCorners);
		}

		// Draw edges
		offset = new Vector2(borderSize - padding, y: 0);
		if (topEdge != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset,
					new Vector2(bounds.Width - (borderSize - padding) * 2, borderSize)),
				topEdge);
		}

		offset = new Vector2(borderSize - padding, bounds.Height - borderSize);
		if (bottomEdge != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset,
					new Vector2(bounds.Width - (borderSize - padding) * 2, borderSize)),
				bottomEdge,
				flipY: flipEdges);
		}

		offset = new Vector2(x: 0, borderSize - padding);
		if (leftEdge != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset,
					new Vector2(borderSize, bounds.Height - (borderSize - padding) * 2)),
				leftEdge,
				flipX: flipEdges,
				flipAxis: flipEdges);
		}

		offset = new Vector2(bounds.Width - borderSize, borderSize - padding);
		if (rightEdge != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset,
					new Vector2(borderSize, bounds.Height - (borderSize - padding) * 2)),
				rightEdge,
				flipY: flipEdges,
				flipAxis: flipEdges);
		}

		// Draw fill
		offset = new Vector2(borderSize - padding, borderSize - padding);
		if (fill != null)
		{
			DrawImage(
				Box2.FromSize(bounds.Location + offset, bounds.Size - offset * 2),
				fill);
		}
	}

	public void DrawImage(Box2 bounds, Texture2D texture,
		float borderTopLeftRadius = default,
		float borderTopRightRadius = default,
		float borderBottomLeftRadius = default,
		float borderBottomRightRadius = default,
		bool flipX = false,
		bool flipY = false,
		bool flipAxis = false)
	{
		if (Camera == null)
		{
			return;
		}

		var prevShader = ActiveShader;
		ActiveShader = shader;
		{
			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
			                     Matrix4.CreateTranslation(bounds.X, convertedY, z: 0);
			shader.HasTexture = true;
			shader.Texture0 = texture;
			shader.FlipX = flipX;
			shader.FlipY = flipY;
			shader.FlipAxis = flipAxis;

			shader.BorderTopLeftRadius = borderTopLeftRadius;
			shader.BorderTopRightRadius = borderTopRightRadius;
			shader.BorderBottomLeftRadius = borderBottomLeftRadius;
			shader.BorderBottomRightRadius = borderBottomRightRadius;
			DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
		}
		ActiveShader = prevShader;
	}

	public void DrawNinePatch(NinePatch ninePatch, Box2 bounds)
	{
		if (Camera == null)
		{
			return;
		}

		var oldZ = Camera.Position.Z;
		Camera.Position = Camera.Position with
		{
			Z = 0,
		};
		spriteBatch.Begin(Camera);
		{
			var vertices = ninePatch.GetVertices(bounds);
			spriteBatch.Draw((Texture2D)ninePatch.Texture, vertices, offset: 0, vertices.Length);
		}
		spriteBatch.End();
		Camera.Position = Camera.Position with
		{
			Z = oldZ,
		};
	}

	public void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		if (Camera == null)
		{
			return;
		}

		var prevShader = ActiveShader;
		var fontShader = font.Shader;
		fontShader.FontAtlas = font.Atlas.Texture;
		fontShader.BackgroundColor = Vector4.Zero;
		fontShader.ForegroundColor = color.ToVector4();
		fontShader.DistanceFieldRange = font.Atlas.DistanceRange;
		fontShader.ProjectionMatrix = Camera.Projection;
		fontShader.ViewMatrix = Camera.View;
		ActiveShader = fontShader;
		{
			var virtualCursor =
				position; // - new Vector2(x: 0, font.ScaleEmToFontSize(font.Metrics.Descender, fontSize));
			// virtualCursor.Y = camera.WorldSize.Y - virtualCursor.Y;
			// var yOffset = font.ScalePixelsToFontSize(camera.WorldSize.Y, fontSize) - ;
			FontGlyph? previousGlyph = null;
			foreach (var glyph in row.Glyphs)
			{
				var lineHeight = font.ScalePixelsToFontSize(row.Size.Y, fontSize);
				virtualCursor = DrawGlyph(font, fontSize, glyph, previousGlyph, virtualCursor, lineHeight);
				previousGlyph = glyph;
			}
		}
		ActiveShader = prevShader;
	}

	public void DrawText(Font font, string text, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		var layout = new TextLayout(font);
		var textRow = new TextRow(font, layout.TextToGlyphs(text));
		DrawText(font, textRow, fontSize, position, color);
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

		var prevShader = ActiveShader;
		ActiveShader = shader;
		{
			var convertedY = Camera.WorldSize.Y - bounds.Size.Y - bounds.Y;
			shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
			                     Matrix4.CreateTranslation(bounds.X, convertedY, z: 0);
			shader.BackgroundColor = color.ToVector4();
			shader.HasTexture = false;
			shader.FlipX = false;
			shader.FlipY = false;
			shader.FlipAxis = false;

			shader.Size = bounds.Size;

			shader.BorderTopLeftRadius = borderTopLeftRadius;
			shader.BorderTopRightRadius = borderTopRightRadius;
			shader.BorderBottomLeftRadius = borderBottomLeftRadius;
			shader.BorderBottomRightRadius = borderBottomRightRadius;

			DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
		}
		ActiveShader = prevShader;
	}

	private Vector2 DrawGlyph(Font font, int fontSize, FontGlyph glyph, FontGlyph? previousGlyph, Vector2 virtualCursor,
		float lineHeight)
	{
		if (Camera == null)
		{
			return Vector2.Zero;
		}

		var fontShader = font.Shader;
		fontShader.UvBounds = RemapUV(font.Atlas.Texture, glyph.AtlasBounds);

		var kerning = previousGlyph == null ? 0 : font.GetKerningBetween(previousGlyph, glyph);
		var offset = font.ScaleEmToFontSize(glyph.Offset, fontSize);
		var location = offset.Location;
		//location.Y = font.ScalePixelsToFontSize(glyph.Size.Y, fontSize) - location.Y;
		location.Y *= -1; // Gotta invert coordinate system 
		location.Y += lineHeight; // Move origin to top left corner instead of bottom left
		var position = virtualCursor + location + new Vector2(kerning, y: 0);
		fontShader.ModelMatrix = Matrix4.CreateScale(offset.Size.X, offset.Size.Y, z: 1) *
		                         Matrix4.CreateTranslation(position.X, Camera.WorldSize.Y - position.Y, z: 0);

		DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);

		var advance = font.ScaleEmToFontSize(glyph.Advance, fontSize);
		return virtualCursor + new Vector2(advance, y: 0);
	}

	private static Vector4 RemapUV(Texture texture, Box2 region)
	{
		var min = region.Min / texture.Dimensions.Xy;
		var max = region.Max / texture.Dimensions.Xy;
		return new Vector4(min.X, min.Y, max.X, max.Y);
	}

	[UsedImplicitly]
	public readonly record struct Vertex(Vector2 Position)
	{
		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			VertexAttribute.Vector2(VertexAttributeLocation.Position)
		);
	}
}