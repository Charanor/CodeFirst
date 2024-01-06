using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.G2D;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public class NinePatch
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
		Texture = region.Texture;
		Center = region;
		StretchableArea = new Box2i(minX: 0, minY: 0, Texture.Width, Texture.Height);
		ContentPadding = Box2i.Empty;
		(middleWidth, middleHeight) = StretchableArea.Size;
		vertices = new SpriteBatch.Vertex[1 * VERTICES_PER_PATCH];
		// Note that technically it IS stretched horizontally and vertically, but in this case our UV:s will
		// range from (0, 0) - (1, 1) so there won't be any artefacts anyways.
		middleCenterIdx = Add(StretchableArea, isStretchedHorizontally: false, isStretchedVertically: false);
	}

	public NinePatch(TextureRegion region, Box2i stretchableArea, Box2i contentPadding)
	{
		Texture = region.Texture;
		StretchableArea = stretchableArea;
		ContentPadding = contentPadding;
		vertices = new SpriteBatch.Vertex[PATCH_COUNT * VERTICES_PER_PATCH];
		
		var x = region.X;
		var y = region.Y;

		var left = stretchableArea.Left;
		var right = region.Width - stretchableArea.Right + x;
		var top = stretchableArea.Top;
		var bottom = region.Height - stretchableArea.Bottom + y;
		var centerWidth = stretchableArea.Width;
		var centerHeight = stretchableArea.Height;

		var bottomLeft = Box2i.FromSize((x, y), (left, top));
		var bottomEdge = Box2i.FromSize((left, y), (centerWidth, top));
		var bottomRight = Box2i.FromSize((left + centerWidth, y), (right, top));

		var leftEdge = Box2i.FromSize((x, top), (left, centerHeight));
		var center = Box2i.FromSize((left, top), (centerWidth, centerHeight));
		var rightEdge = Box2i.FromSize((left + centerWidth, top), (right, centerHeight));

		var topLeft = Box2i.FromSize((x, top + centerHeight), (left, bottom));
		var topEdge = Box2i.FromSize((left, top + centerHeight), (centerWidth, bottom));
		var topRight = Box2i.FromSize((left + centerWidth, top + centerHeight), (right, bottom));

		TopLeft = new TextureRegion(region, topLeft);
		TopEdge = new TextureRegion(region, topEdge);
		TopRight = new TextureRegion(region, topRight);

		LeftEdge = new TextureRegion(region, leftEdge);
		Center = new TextureRegion(region, center);
		RightEdge = new TextureRegion(region, rightEdge);

		BottomLeft = new TextureRegion(region, bottomLeft);
		BottomEdge = new TextureRegion(region, bottomEdge);
		BottomRight = new TextureRegion(region, bottomRight);

		if (left == 0 && centerWidth == 0)
		{
			topEdge = topRight;
			TopEdge = TopRight;
			center = rightEdge;
			Center = RightEdge;
			bottomEdge = bottomRight;
			BottomEdge = BottomRight;

			topRight = default;
			TopRight = null;
			rightEdge = default;
			RightEdge = null;
			bottomRight = default;
			BottomRight = null;
		}

		if (top == 0 && centerHeight == 0)
		{
			leftEdge = bottomLeft;
			LeftEdge = BottomLeft;
			center = bottomEdge;
			Center = BottomEdge;
			rightEdge = bottomRight;
			RightEdge = BottomRight;

			bottomLeft = default;
			BottomLeft = null;
			bottomEdge = default;
			BottomEdge = null;
			bottomRight = default;
			BottomRight = null;
		}

		if (!bottomLeft.IsZero)
		{
			bottomLeftIdx = Add(bottomLeft, isStretchedHorizontally: false, isStretchedVertically: false);
			leftWidth = bottomLeft.Width;
			bottomHeight = bottomLeft.Height;
		}

		if (!bottomEdge.IsZero)
		{
			bottomCenterIdx = Add(bottomEdge, !bottomLeft.IsZero || !bottomRight.IsZero, isStretchedVertically: false);
			middleWidth = Math.Max(middleWidth, bottomEdge.Width);
			bottomHeight = Math.Max(bottomHeight, bottomEdge.Height);
		}

		if (!bottomRight.IsZero)
		{
			bottomRightIdx = Add(bottomRight, isStretchedHorizontally: false, isStretchedVertically: false);
			rightWidth = Math.Max(rightWidth, bottomRight.Width);
			bottomHeight = Math.Max(bottomHeight, bottomRight.Height);
		}

		if (!leftEdge.IsZero)
		{
			middleLeftIdx = Add(leftEdge, isStretchedHorizontally: false, !topLeft.IsZero || !bottomLeft.IsZero);
			leftWidth = Math.Max(leftWidth, leftEdge.Width);
			middleHeight = Math.Max(middleHeight, leftEdge.Height);
		}

		if (!center.IsZero)
		{
			middleCenterIdx = Add(center, !leftEdge.IsZero || !rightEdge.IsZero, !topEdge.IsZero || !bottomEdge.IsZero);
			middleWidth = Math.Max(middleWidth, center.Width);
			middleHeight = Math.Max(middleHeight, center.Height);
		}

		if (!rightEdge.IsZero)
		{
			middleRightIdx = Add(rightEdge, isStretchedHorizontally: false, !topRight.IsZero || !bottomRight.IsZero);
			rightWidth = Math.Max(rightWidth, rightEdge.Width);
			middleHeight = Math.Max(middleHeight, rightEdge.Height);
		}

		if (!topLeft.IsZero)
		{
			topLeftIdx = Add(topLeft, isStretchedHorizontally: false, isStretchedVertically: false);
			leftWidth = Math.Max(leftWidth, topLeft.Width);
			topHeight = Math.Max(topHeight, topLeft.Height);
		}

		if (!topEdge.IsZero)
		{
			topCenterIdx = Add(topEdge, !topLeft.IsZero || !topRight.IsZero, isStretchedVertically: false);
			middleWidth = Math.Max(middleWidth, topEdge.Width);
			topHeight = Math.Max(topHeight, topEdge.Height);
		}

		if (!topRight.IsZero)
		{
			topRightIdx = Add(topRight, isStretchedHorizontally: false, isStretchedVertically: false);
			rightWidth = Math.Max(rightWidth, topRight.Width);
			topHeight = Math.Max(topHeight, topRight.Height);
		}

		if (idx < vertices.Length)
		{
			Array.Resize(ref vertices, idx);
		}
	}

	public Texture Texture { get; }

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

	public Box2i StretchableArea { get; }
	public Box2i ContentPadding { get; }

	public Box2i ContentArea => new(
		ContentPadding.Left,
		ContentPadding.Top,
		Texture.Width - ContentPadding.Right,
		Texture.Height - ContentPadding.Bottom
	);

	private int Add(Box2i region, bool isStretchedHorizontally, bool isStretchedVertically)
	{
		var uv = new Box2(region.Min / (Vector2)Texture.Dimensions.Xy, region.Max / (Vector2)Texture.Dimensions.Xy);
		var (u, v) = uv.Min;
		var (u2, v2) = uv.Max;

		if (Texture.MagFilter == TextureMagFilter.Linear || Texture.MinFilter == TextureMinFilter.Linear)
		{
			// Add half pixel offsets on stretchable dimensions to avoid color bleeding when GL_LINEAR
			// filtering is used for the texture. This nudges the texture coordinate to the center
			// of the texel where the neighboring pixel has 0% contribution in linear blending mode.
			if (isStretchedHorizontally)
			{
				var halfTexelWidth = 0.5f * 1f / Texture.Width;
				u += halfTexelWidth;
				u2 -= halfTexelWidth;
			}

			if (isStretchedVertically)
			{
				var halfTexelHeight = 0.5f * 1f / Texture.Height;
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

	/**
	 * Set the coordinates and color of a ninth of the patch.
	 */
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

	public SpriteBatch.Vertex[] GetVertices(Box2 bounds, Color4<Rgba> color = default)
	{
		color = color == default ? Color4.White : color;
		
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