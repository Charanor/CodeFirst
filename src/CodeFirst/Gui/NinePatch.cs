using CodeFirst.Graphics.Core;
using CodeFirst.Graphics.G2D;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public class NinePatch
{
	private const int PATCH_COUNT = 9;
	private const int VERTICES_PER_PATCH = 4;

	private readonly int topLeftIdx;
	private readonly int topCenterIdx;
	private readonly int topRightIdx;

	private readonly int middleLeftIdx;
	private readonly int middleCenterIdx;
	private readonly int middleRightIdx;

	private readonly int bottomLeftIdx;
	private readonly int bottomCenterIdx;
	private readonly int bottomRightIdx;

	private readonly float leftWidth;
	private readonly float rightWidth;
	private readonly float middleWidth;
	private readonly float middleHeight;
	private readonly float topHeight;
	private readonly float bottomHeight;

	private readonly SpriteBatch.Vertex[] vertices;

	private int idx;

	public NinePatch(TextureRegion region, Box2i stretchableArea, Box2i contentPadding)
	{
		Texture = region.Texture;
		StretchableArea = stretchableArea;
		ContentPadding = contentPadding;
		vertices = new SpriteBatch.Vertex[PATCH_COUNT * VERTICES_PER_PATCH];

		var left = stretchableArea.Left;
		var right = region.Width - stretchableArea.Right;
		var top = region.Height - stretchableArea.Top;
		var bottom = stretchableArea.Bottom;
		var centerWidth = region.Width - left - right;
		var centerHeight = region.Height - top - bottom;

		var bottomLeft = Box2i.FromSize(Vector2i.Zero, (left, top));
		var bottomEdge = Box2i.FromSize((left, 0), (centerWidth, top));
		var bottomRight = Box2i.FromSize((left + centerWidth, 0), (right, top));

		var leftEdge = Box2i.FromSize((0, top), (left, centerHeight));
		var center = Box2i.FromSize((left, top), (centerWidth, centerHeight));
		var rightEdge = Box2i.FromSize((left + centerWidth, top), (right, centerHeight));

		var topLeft = Box2i.FromSize((0, top + centerHeight), (left, bottom));
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
			bottomLeftIdx = Add(bottomLeft, isStretchW: false, isStretchH: false);
			leftWidth = bottomLeft.Width;
			bottomHeight = bottomLeft.Height;
		}
		else
		{
			bottomLeftIdx = -1;
		}

		if (!bottomEdge.IsZero)
		{
			bottomCenterIdx = Add(bottomEdge, !bottomLeft.IsZero || !bottomRight.IsZero, isStretchH: false);
			middleWidth = Math.Max(middleWidth, bottomEdge.Width);
			bottomHeight = Math.Max(bottomHeight, bottomEdge.Height);
		}
		else
		{
			bottomCenterIdx = -1;
		}

		if (!bottomRight.IsZero)
		{
			bottomRightIdx = Add(bottomRight, isStretchW: false, isStretchH: false);
			rightWidth = Math.Max(rightWidth, bottomRight.Width);
			bottomHeight = Math.Max(bottomHeight, bottomRight.Height);
		}
		else
		{
			bottomRightIdx = -1;
		}

		if (!leftEdge.IsZero)
		{
			middleLeftIdx = Add(leftEdge, isStretchW: false, !topLeft.IsZero || !bottomLeft.IsZero);
			leftWidth = Math.Max(leftWidth, leftEdge.Width);
			middleHeight = Math.Max(middleHeight, leftEdge.Height);
		}
		else
		{
			middleLeftIdx = -1;
		}

		if (!center.IsZero)
		{
			middleCenterIdx = Add(center, !leftEdge.IsZero || !rightEdge.IsZero, !topEdge.IsZero || !bottomEdge.IsZero);
			middleWidth = Math.Max(middleWidth, center.Width);
			middleHeight = Math.Max(middleHeight, center.Height);
		}
		else
		{
			middleCenterIdx = -1;
		}

		if (!rightEdge.IsZero)
		{
			middleRightIdx = Add(rightEdge, isStretchW: false, !topRight.IsZero || !bottomRight.IsZero);
			rightWidth = Math.Max(rightWidth, rightEdge.Width);
			middleHeight = Math.Max(middleHeight, rightEdge.Height);
		}
		else
		{
			middleRightIdx = -1;
		}

		if (!topLeft.IsZero)
		{
			topLeftIdx = Add(topLeft, isStretchW: false, isStretchH: false);
			leftWidth = Math.Max(leftWidth, topLeft.Width);
			topHeight = Math.Max(topHeight, topLeft.Height);
		}
		else
		{
			topLeftIdx = -1;
		}

		if (!topEdge.IsZero)
		{
			topCenterIdx = Add(topEdge, !topLeft.IsZero || !topRight.IsZero, isStretchH: false);
			middleWidth = Math.Max(middleWidth, topEdge.Width);
			topHeight = Math.Max(topHeight, topEdge.Height);
		}
		else
		{
			topCenterIdx = -1;
		}

		if (!topRight.IsZero)
		{
			topRightIdx = Add(topRight, isStretchW: false, isStretchH: false);
			rightWidth = Math.Max(rightWidth, topRight.Width);
			topHeight = Math.Max(topHeight, topRight.Height);
		}
		else
		{
			topRightIdx = -1;
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

	private int Add(Box2i region, bool isStretchW, bool isStretchH)
	{
		var uv = new Box2(region.Min / (Vector2)Texture.Dimensions.Xy, region.Max / (Vector2)Texture.Dimensions.Xy);
		var (u, v) = uv.Min;
		var (u2, v2) = uv.Max;

		if (Texture.MagFilter == TextureMagFilter.Linear || Texture.MinFilter == TextureMinFilter.Linear)
		{
			// Add half pixel offsets on stretchable dimensions to avoid color bleeding when GL_LINEAR
			// filtering is used for the texture. This nudges the texture coordinate to the center
			// of the texel where the neighboring pixel has 0% contribution in linear blending mode.
			if (isStretchW)
			{
				var halfTexelWidth = 0.5f * 1f / Texture.Width;
				u += halfTexelWidth;
				u2 -= halfTexelWidth;
			}

			if (isStretchH)
			{
				var halfTexelHeight = 0.5f * 1f / Texture.Height;
				v -= halfTexelHeight;
				v2 += halfTexelHeight;
			}
		}

		var i = idx;
		vertices[i] = vertices[i] with
		{
			UVCoordinates = new Vector2(u, v)
		};
		vertices[i + 1] = vertices[i + 1] with
		{
			UVCoordinates = new Vector2(u2, v)
		};
		vertices[i + 2] = vertices[i + 2] with
		{
			UVCoordinates = new Vector2(u2, v2)
		};
		vertices[i + 3] = vertices[i + 3] with
		{
			UVCoordinates = new Vector2(u, v2)
		};

		idx += VERTICES_PER_PATCH;
		return i;
	}

	/**
	 * Set the coordinates and color of a ninth of the patch.
	 */
	private void Set(int index, float x, float y, float width, float height)
	{
		vertices[index] = vertices[index] with
		{
			Position = (x, y, -1)
		};
		vertices[index + 1] = vertices[index + 1] with
		{
			Position = (x + width, y, -1)
		};
		vertices[index + 2] = vertices[index + 2] with
		{
			Position = (x + width, y + height, -1)
		};
		vertices[index + 3] = vertices[index + 3] with
		{
			Position = (x, y + height, -1)
		};
	}

	public SpriteBatch.Vertex[] GetVertices(Box2 bounds)
	{
		var x = bounds.X;
		var y = bounds.Y;
		var width = bounds.Width;
		var height = bounds.Height;

		var centerX = x + leftWidth;
		var centerY = y + bottomHeight;
		var centerWidth = width - rightWidth - leftWidth;
		var centerHeight = height - topHeight - bottomHeight;
		var rightX = x + width - rightWidth;
		var topY = y + height - topHeight;

		if (bottomLeftIdx != -1)
		{
			Set(bottomLeftIdx, x, y, leftWidth, bottomHeight);
		}

		if (bottomCenterIdx != -1)
		{
			Set(bottomCenterIdx, centerX, y, centerWidth, bottomHeight);
		}

		if (bottomRightIdx != -1)
		{
			Set(bottomRightIdx, rightX, y, rightWidth, bottomHeight);
		}

		if (middleLeftIdx != -1)
		{
			Set(middleLeftIdx, x, centerY, leftWidth, centerHeight);
		}

		if (middleCenterIdx != -1)
		{
			Set(middleCenterIdx, centerX, centerY, centerWidth, centerHeight);
		}

		if (middleRightIdx != -1)
		{
			Set(middleRightIdx, rightX, centerY, rightWidth, centerHeight);
		}

		if (topLeftIdx != -1)
		{
			Set(topLeftIdx, x, topY, leftWidth, topHeight);
		}

		if (topCenterIdx != -1)
		{
			Set(topCenterIdx, centerX, topY, centerWidth, topHeight);
		}

		if (topRightIdx != -1)
		{
			Set(topRightIdx, rightX, topY, rightWidth, topHeight);
		}

		return vertices;
	}
}