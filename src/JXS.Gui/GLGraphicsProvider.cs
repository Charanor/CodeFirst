using System.Diagnostics;
using JXS.Graphics.Core;
using JXS.Graphics.Generated;
using JXS.Graphics.Text;
using JXS.Graphics.Text.Layout;
using JXS.Graphics.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace JXS.Gui;

public class GLGraphicsProvider : IGraphicsProvider, IDisposable
{
	private static readonly DemoVertex[] QuadVertices =
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

	private readonly Camera camera;

	private readonly Buffer<DemoVertex> vertexBuffer;
	private readonly Buffer<uint> indexBuffer;
	private readonly VertexArray vertexArray;
	private readonly BasicGraphicsShader shader;

	private int overflowLayer;

	private ShaderProgram? activeShader;

	public GLGraphicsProvider(Camera camera)
	{
		this.camera = camera;
		vertexBuffer = new Buffer<DemoVertex>(QuadVertices, VertexBufferObjectUsage.StaticRead);
		indexBuffer = new Buffer<uint>(QuadIndices, VertexBufferObjectUsage.StaticRead);
		vertexArray = VertexArray.CreateForVertexInfo(DemoVertex.VertexInfo, vertexBuffer, indexBuffer);
		shader = new BasicGraphicsShader();
		overflowLayer = 0;
	}

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

	public void Begin()
	{
		GL.Enable(EnableCap.Blend);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.Clear(ClearBufferMask.StencilBufferBit);
		GL.Enable(EnableCap.StencilTest);
		GL.StencilFunc(StencilFunction.Always, reference: 1, mask: 0xff);
		GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
		GL.StencilMask(0xff);

		shader.ProjectionMatrix = camera.Projection;
		shader.ViewMatrix = camera.View;
		ActiveShader = shader;

		vertexArray.Bind();
	}

	public void End()
	{
		Debug.Assert(overflowLayer == 0);
		vertexArray.Unbind();
		ActiveShader = null;

		GL.Clear(ClearBufferMask.StencilBufferBit);
		GL.Disable(EnableCap.StencilTest);
		GL.Disable(EnableCap.Blend);
	}

	public void BeginOverflow()
	{
		overflowLayer += 1;
		GL.StencilFunc(StencilFunction.Lequal, overflowLayer, mask: 0xff);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
	}

	public void EndOverflow()
	{
		overflowLayer -= 1;
		GL.StencilFunc(StencilFunction.Lequal, overflowLayer, mask: 0xff);
	}

	public void DrawImage(Box2 bounds, Texture2D texture)
	{
		var prevShader = ActiveShader;
		ActiveShader = shader;
		{
			shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
			                     Matrix4.CreateTranslation(bounds.X, bounds.Y, z: 0);
			shader.HasTexture = true;
			shader.Texture0 = texture;
			GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
		}
		ActiveShader = prevShader;
	}

	public void DrawText(Font font, TextRow row, int fontSize, Vector2 position, Color4<Rgba> color)
	{
		var prevShader = ActiveShader;
		var fontShader = font.Shader;
		fontShader.FontAtlas = font.Atlas.Texture;
		fontShader.BackgroundColor = Vector4.Zero;
		fontShader.ForegroundColor = color.ToVector4();
		fontShader.DistanceFieldRange = font.Atlas.DistanceRange;
		fontShader.ProjectionMatrix = camera.Projection;
		fontShader.ViewMatrix = camera.View;
		ActiveShader = fontShader;
		{
			var virtualCursor = position - new Vector2(x: 0, font.ScaleEmToFontSize(font.Metrics.Descender, fontSize));
			FontGlyph? previousGlyph = null;
			foreach (var glyph in row.Glyphs)
			{
				virtualCursor = DrawGlyph(font, fontSize, glyph, previousGlyph, virtualCursor);
				previousGlyph = glyph;
			}
		}
		ActiveShader = prevShader;
	}

	public void DrawRect(Box2 bounds, Color4<Rgba> color, float borderTop = default, float borderBottom = default,
		float borderLeft = default, float borderRight = default, Color4<Rgba> borderColor = default)
	{
		var prevShader = ActiveShader;
		ActiveShader = shader;
		{
			shader.ModelMatrix = Matrix4.CreateScale(bounds.Size.X, bounds.Size.Y, z: 1) *
			                     Matrix4.CreateTranslation(bounds.X, bounds.Y, z: 0);
			shader.BackgroundColor = color.ToVector4();
			shader.BorderLeft = borderLeft / bounds.Size.X;
			shader.BorderRight = borderRight / bounds.Size.X;
			shader.BorderTop = borderTop / bounds.Size.Y;
			shader.BorderBottom = borderBottom / bounds.Size.Y;
			shader.BorderColor = borderColor.ToVector4();
			shader.HasTexture = false;
			GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);
		}
		ActiveShader = prevShader;
	}

	private Vector2 DrawGlyph(Font font, int fontSize, FontGlyph glyph, FontGlyph? previousGlyph, Vector2 virtualCursor)
	{
		var fontShader = font.Shader;
		fontShader.UvBounds = RemapUV(font.Atlas.Texture, glyph.AtlasBounds);

		var kerning = previousGlyph == null ? 0 : font.GetKerningBetween(previousGlyph, glyph);
		var offset = font.ScaleEmToFontSize(glyph.Offset, fontSize);
		var position = virtualCursor + offset.Location + new Vector2(kerning, y: 0);
		fontShader.ModelMatrix = Matrix4.CreateScale(offset.Size.X, offset.Size.Y, z: 1) *
		                         Matrix4.CreateTranslation(position.X, position.Y, z: 0);

		GL.DrawElements(PrimitiveType.Triangles, QuadIndices.Length, DrawElementsType.UnsignedInt, offset: 0);

		var advance = font.ScaleEmToFontSize(glyph.Advance, fontSize);
		return virtualCursor + new Vector2(advance, y: 0);
	}

	private static Vector4 RemapUV(Texture texture, Box2 region)
	{
		var min = region.Min / texture.Dimensions.Xy;
		var max = region.Max / texture.Dimensions.Xy;
		return new Vector4(min.X, min.Y, max.X, max.Y);
	}

	public readonly record struct DemoVertex(Vector2 Position)
	{
		public static readonly VertexAttribute PositionAttribute =
			new(VertexAttributeLocation.Position, ComponentCount: 2, sizeof(float), VertexAttribType.Float,
				Offset: 0);

		public static readonly VertexInfo VertexInfo = new(
			typeof(Vertex),
			PositionAttribute
		);
	}
}