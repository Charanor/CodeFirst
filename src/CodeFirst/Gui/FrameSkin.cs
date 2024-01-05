using System.Diagnostics.CodeAnalysis;
using CodeFirst.Graphics.Core;

namespace CodeFirst.Gui;

public readonly struct FrameSkin
{
	private readonly bool flipCorners;
	private readonly bool flipEdges;

	private readonly Texture2D? topLeft = null;
	private readonly Texture2D? topRight = null;
	private readonly Texture2D? bottomLeft = null;
	private readonly Texture2D? bottomRight = null;
	private readonly Texture2D? topEdge = null;
	private readonly Texture2D? bottomEdge = null;
	private readonly Texture2D? leftEdge = null;
	private readonly Texture2D? rightEdge = null;
	private readonly Texture2D? background = null;

	private readonly float size;

	public FrameSkin(Texture2D background, float size = 0)
	{
		this.background = background;
		this.size = size;
	}

	public FrameSkin(Texture2D background, Texture2D corners, Texture2D edges, float size = 0) : this(background, corners, edges, corners, corners, edges, corners, edges, edges, size)
	{
		flipCorners = true;
		flipEdges = true;
	}

	public FrameSkin(Texture2D background, Texture2D topLeft, Texture2D topEdge, Texture2D topRight, Texture2D bottomLeft,Texture2D bottomEdge,Texture2D bottomRight,Texture2D leftEdge,Texture2D rightEdge, float size = 0)
	{
		this.background = background;
		this.topLeft = topLeft;
		this.topEdge = topEdge;
		this.topRight = topRight;
		this.bottomLeft = bottomLeft;
		this.bottomEdge = bottomEdge;
		this.bottomRight = bottomRight;
		this.leftEdge = leftEdge;
		this.rightEdge = rightEdge;
		this.size = size;
	}

	[MemberNotNullWhen(returnValue: true, nameof(background))]
	private bool HasBackground => background != null;

	private bool HasBorder => topLeft != null || topRight != null ||
	                          bottomLeft != null || bottomRight != null ||
	                          leftEdge != null || rightEdge != null ||
	                          topEdge != null || bottomEdge != null;

	public void Draw(Frame frame, IGraphicsProvider graphicsProvider)
	{
		var bounds = frame.TransformedBounds;
		if (!HasBorder)
		{
			if (!HasBackground)
			{
				// Nothing to draw!
				return;
			}

			// Draw background
			var (topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius) = frame.BorderRadii;
			graphicsProvider.DrawImage(bounds, background, topLeftRadius, topRightRadius, bottomLeftRadius,
				bottomRightRadius);
			return;
		}

		var (bottom, left, right, top) = frame.BorderSize;
		var borderSize = size <= 0 ? MathF.Max(MathF.Max(left, right), MathF.Max(top, bottom)) : size;
		graphicsProvider.DrawWindow(bounds, borderSize,
			topLeft, topEdge, topRight,
			bottomLeft, bottomEdge, bottomRight,
			leftEdge, rightEdge, background,
			flipCorners: flipCorners, flipEdges: flipEdges);
	}
}