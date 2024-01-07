using CodeFirst.Graphics.Core;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public class NinePatch : IDrawable
{
	private const int PATCH_COUNT = 9;
	private const int VERTICES_PER_PATCH = 4;

	private readonly int topLeftIdx = -1;
	private readonly int topCenterIdx = -1;
	private readonly int topRightIdx = -1;

	private readonly int middleLeftIdx = -1;
	private readonly int middleCenterIdx = -1;
	private readonly int middleRightIdx = -1;

	private readonly int bottomLeftIdx = -1;
	private readonly int bottomCenterIdx = -1;
	private readonly int bottomRightIdx = -1;

	private readonly float leftWidth;
	private readonly float rightWidth;
	private readonly float middleWidth;
	private readonly float middleHeight;
	private readonly float topHeight;
	private readonly float bottomHeight;

	private readonly SpriteBatch.Vertex[] vertices;

	private int idx;

	public NinePatch(TextureRegion region)
	{
		TextureRegion = region;
		Center = region;
		Insets = Box2i.Empty;
		Padding = Box2i.Empty;
		(middleWidth, middleHeight) = region.Size2D;
		vertices = new SpriteBatch.Vertex[1 * VERTICES_PER_PATCH];
		// Note that technically it IS stretched horizontally and vertically, but in this case our UV:s will
		// range from (0, 0) - (1, 1) so there won't be any artefacts anyways.
		middleCenterIdx = Add(Center, isStretchedHorizontally: false, isStretchedVertically: false);
	}

	public NinePatch(TextureRegion region, Box2i insets, Box2i padding)
	{
		TextureRegion = region;
		Insets = insets;
		Padding = padding;
		vertices = new SpriteBatch.Vertex[PATCH_COUNT * VERTICES_PER_PATCH];

		var left = insets.Left;
		var right = insets.Right;
		var top = insets.Top;
		var bottom = insets.Bottom;
		var centerWidth = region.Width - left - right;
		var centerHeight = region.Height - top - bottom;

		TopLeft = new TextureRegion(region, Box2i.FromSize((0, top + centerHeight), (left, bottom)));
		TopEdge = new TextureRegion(region, Box2i.FromSize((left, top + centerHeight), (centerWidth, bottom)));
		TopRight = new TextureRegion(region, Box2i.FromSize((left + centerWidth, top + centerHeight), (right, bottom)));

		LeftEdge = new TextureRegion(region, Box2i.FromSize((0, top), (left, centerHeight)));
		Center = new TextureRegion(region, Box2i.FromSize((left, top), (centerWidth, centerHeight)));
		RightEdge = new TextureRegion(region, Box2i.FromSize((left + centerWidth, top), (right, centerHeight)));

		BottomLeft = new TextureRegion(region, Box2i.FromSize((0, 0), (left, top)));
		BottomEdge = new TextureRegion(region, Box2i.FromSize((left, 0), (centerWidth, top)));
		BottomRight = new TextureRegion(region, Box2i.FromSize((left + centerWidth, 0), (right, top)));

		if (left == 0 && centerWidth == 0)
		{
			TopEdge = TopRight;
			Center = RightEdge;
			BottomEdge = BottomRight;

			TopRight = null;
			RightEdge = null;
			BottomRight = null;
		}

		if (top == 0 && centerHeight == 0)
		{
			LeftEdge = BottomLeft;
			Center = BottomEdge;
			RightEdge = BottomRight;

			BottomLeft = null;
			BottomEdge = null;
			BottomRight = null;
		}

		if (BottomLeft != null)
		{
			bottomLeftIdx = Add(BottomLeft, isStretchedHorizontally: false, isStretchedVertically: false);
			leftWidth = BottomLeft.Width;
			bottomHeight = BottomLeft.Height;
		}

		if (BottomEdge != null)
		{
			bottomCenterIdx = Add(BottomEdge, BottomLeft != null || BottomRight != null, isStretchedVertically: false);
			middleWidth = Math.Max(middleWidth, BottomEdge.Width);
			bottomHeight = Math.Max(bottomHeight, BottomEdge.Height);
		}

		if (BottomRight != null)
		{
			bottomRightIdx = Add(BottomRight, isStretchedHorizontally: false, isStretchedVertically: false);
			rightWidth = Math.Max(rightWidth, BottomRight.Width);
			bottomHeight = Math.Max(bottomHeight, BottomRight.Height);
		}

		if (LeftEdge != null)
		{
			middleLeftIdx = Add(LeftEdge, isStretchedHorizontally: false, TopLeft != null || BottomLeft != null);
			leftWidth = Math.Max(leftWidth, LeftEdge.Width);
			middleHeight = Math.Max(middleHeight, LeftEdge.Height);
		}

		if (Center != null)
		{
			middleCenterIdx = Add(Center, LeftEdge != null || RightEdge != null, TopEdge != null || BottomEdge != null);
			middleWidth = Math.Max(middleWidth, Center.Width);
			middleHeight = Math.Max(middleHeight, Center.Height);
		}

		if (RightEdge != null)
		{
			middleRightIdx = Add(RightEdge, isStretchedHorizontally: false, TopRight != null || BottomRight != null);
			rightWidth = Math.Max(rightWidth, RightEdge.Width);
			middleHeight = Math.Max(middleHeight, RightEdge.Height);
		}

		if (TopLeft != null)
		{
			topLeftIdx = Add(TopLeft, isStretchedHorizontally: false, isStretchedVertically: false);
			leftWidth = Math.Max(leftWidth, TopLeft.Width);
			topHeight = Math.Max(topHeight, TopLeft.Height);
		}

		if (TopEdge != null)
		{
			topCenterIdx = Add(TopEdge, TopLeft != null || TopRight != null, isStretchedVertically: false);
			middleWidth = Math.Max(middleWidth, TopEdge.Width);
			topHeight = Math.Max(topHeight, TopEdge.Height);
		}

		if (TopRight != null)
		{
			topRightIdx = Add(TopRight, isStretchedHorizontally: false, isStretchedVertically: false);
			rightWidth = Math.Max(rightWidth, TopRight.Width);
			topHeight = Math.Max(topHeight, TopRight.Height);
		}

		if (idx < vertices.Length)
		{
			Array.Resize(ref vertices, idx);
		}
	}

	public TextureRegion TextureRegion { get; }

	// Corners
	public TextureRegion? TopLeft { get; }
	public TextureRegion? TopRight { get; }
	public TextureRegion? BottomLeft { get; }
	public TextureRegion? BottomRight { get; }

	// Edges
	public TextureRegion? LeftEdge { get; }
	public TextureRegion? RightEdge { get; }
	public TextureRegion? TopEdge { get; }
	public TextureRegion? BottomEdge { get; }

	// Center
	public TextureRegion? Center { get; }

	public Box2i Insets { get; }
	public Box2i Padding { get; }

	public void Draw(SpriteBatch batch, Box2 region, Color4<Rgba> color)
	{
		using (batch.SetTemporaryShader(null))
		{
			var camera = batch.Camera;
			var verts = GetVertices(region, color).Select(vert => vert with
			{
				Position = vert.Position with
				{
					Y = (camera?.WorldSize.Y ?? 0) - vert.Position.Y
				}
			}).ToArray();
			batch.Draw((Texture2D)TextureRegion.Texture, verts, offset: 0, verts.Length);
		}
	}

	private int Add(TextureRegion region, bool isStretchedHorizontally, bool isStretchedVertically)
	{
		// var uv = region.UVBounds2D;
		var texture = TextureRegion.Texture;
		var uv = new Box2(
			region.Bounds2D.Min / (Vector2)texture.Dimensions.Xy,
			// I cannot quite figure out why we need to add 1 to width and height to get the correct UVs...
			// Note that it isn't related to the offset of the region or the root texture region, it's always 1px off...
			(region.Bounds2D.Max + (1, 1)) / (Vector2)texture.Dimensions.Xy
		);
		var (u, v) = uv.Min;
		var (u2, v2) = uv.Max;

		if (texture.MagFilter == TextureMagFilter.Linear || texture.MinFilter == TextureMinFilter.Linear)
		{
			// Add half pixel offsets on stretchable dimensions to avoid color bleeding when GL_LINEAR
			// filtering is used for the texture. This nudges the texture coordinate to the center
			// of the texel where the neighboring pixel has 0% contribution in linear blending mode.
			if (isStretchedHorizontally)
			{
				var halfTexelWidth = 0.5f * 1f / texture.Width;
				u += halfTexelWidth;
				u2 -= halfTexelWidth;
			}

			if (isStretchedVertically)
			{
				var halfTexelHeight = 0.5f * 1f / texture.Height;
				v -= halfTexelHeight;
				v2 += halfTexelHeight;
			}
		}

		var i = idx;
		vertices[i] = vertices[i] with
		{
			UVCoordinates = new Vector2(u, v2)
		};
		vertices[i + 1] = vertices[i + 1] with
		{
			UVCoordinates = new Vector2(u2, v2)
		};
		vertices[i + 2] = vertices[i + 2] with
		{
			UVCoordinates = new Vector2(u2, v)
		};
		vertices[i + 3] = vertices[i + 3] with
		{
			UVCoordinates = new Vector2(u, v)
		};

		idx += VERTICES_PER_PATCH;
		return i;
	}

	private void Set(int index, float x, float y, float width, float height, Color4<Rgba> color)
	{
		vertices[index] = vertices[index] with
		{
			Position = (x, y, -1),
			Color = color
		};
		vertices[index + 1] = vertices[index + 1] with
		{
			Position = (x + width, y, -1),
			Color = color
		};
		vertices[index + 2] = vertices[index + 2] with
		{
			Position = (x + width, y + height, -1),
			Color = color
		};
		vertices[index + 3] = vertices[index + 3] with
		{
			Position = (x, y + height, -1),
			Color = color
		};
	}

	private IEnumerable<SpriteBatch.Vertex> GetVertices(Box2 bounds, Color4<Rgba> color)
	{
		var height = bounds.Height;
		var centerHeight = height - topHeight - bottomHeight;

		var topY = bounds.Top;
		var centerY = topY + topHeight;
		var bottomY = centerY + centerHeight;

		var width = bounds.Width;
		var centerWidth = width - rightWidth - leftWidth;

		var leftX = bounds.Left;
		var centerX = leftX + leftWidth;
		var rightX = centerX + centerWidth;

		if (bottomLeftIdx != -1)
		{
			Set(bottomLeftIdx, leftX, bottomY, leftWidth, bottomHeight, color);
		}

		if (bottomCenterIdx != -1)
		{
			Set(bottomCenterIdx, centerX, bottomY, centerWidth, bottomHeight, color);
		}

		if (bottomRightIdx != -1)
		{
			Set(bottomRightIdx, rightX, bottomY, rightWidth, bottomHeight, color);
		}

		if (middleLeftIdx != -1)
		{
			Set(middleLeftIdx, leftX, centerY, leftWidth, centerHeight, color);
		}

		if (middleCenterIdx != -1)
		{
			Set(middleCenterIdx, centerX, centerY, centerWidth, centerHeight, color);
		}

		if (middleRightIdx != -1)
		{
			Set(middleRightIdx, rightX, centerY, rightWidth, centerHeight, color);
		}

		if (topLeftIdx != -1)
		{
			Set(topLeftIdx, leftX, topY, leftWidth, topHeight, color);
		}

		if (topCenterIdx != -1)
		{
			Set(topCenterIdx, centerX, topY, centerWidth, topHeight, color);
		}

		if (topRightIdx != -1)
		{
			Set(topRightIdx, rightX, topY, rightWidth, topHeight, color);
		}

		return vertices;
	}

	public static implicit operator NinePatch(Texture texture) => new(texture);
	public static implicit operator NinePatch(TextureRegion region) => new(region);
}