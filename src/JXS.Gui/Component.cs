using Facebook.Yoga;
using JXS.Gui.Components;
using OpenTK.Mathematics;

namespace JXS.Gui;

public abstract class Component
{
	protected internal readonly YogaNode Node;
	private Scene? scene;

	protected Component()
	{
		Node = new YogaNode
		{
			Data = this
		};
		Transform = new Transform();
	}

	public Transform Transform { get; }

	public abstract bool Visible { get; }
	public abstract YogaOverflow Overflow { get; }
	public abstract Color4<Rgba> BackgroundColor { get; }

	public abstract (float left, float right, float top, float bottom) BorderSize { get; }
	public abstract (float topLeft, float topRight, float bottomLeft, float bottomRight) BorderRadii { get; }

	public abstract Color4<Rgba> BorderColor { get; }

	public string? Id { get; init; }

	protected IGuiInputProvider? InputProvider => Scene?.GuiInputProvider;

	/// <summary>
	///     The parent of this Component (if any).
	/// </summary>
	public View? Parent { get; protected internal set; }

	public Scene? Scene
	{
		get => scene ?? Parent?.Scene;
		protected internal set
		{
			if (scene == value)
			{
				return;
			}
			
			OnRemovedFromScene();
			scene = value;
			if (scene != null)
			{
				OnAddedToScene();
			}
		}
	}
	
	protected virtual void OnAddedToScene() {}
	protected virtual void OnRemovedFromScene() {}

	/// <summary>
	///     The bounds (x, y, width, height) of this component as calculated by <see cref="CalculateLayout" />.
	///     This is used for layout purposes, you are probably looking for <see cref="TransformedBounds" /> instead.
	/// </summary>
	/// <seealso cref="TransformedBounds" />
	/// <seealso cref="CalculateLayout" />
	public Box2 CalculatedBounds
	{
		get
		{
			var x = Parent is null ? Node.LayoutX : Node.LayoutX + Parent.CalculatedBounds.X;
			var y = Parent is null ? Node.LayoutY : Node.LayoutY + Parent.CalculatedBounds.Y;
			return Box2.FromSize(new Vector2(x, y), new Vector2(Node.LayoutWidth, Node.LayoutHeight));
		}
	}

	/// <summary>
	///     The bounds after applying the transform of this component. Use this for rendering and hit detection etc.
	/// </summary>
	/// <seealso cref="CalculatedBounds" />
	/// <seealso cref="CalculateTotalTransform" />
	public Box2 TransformedBounds => CalculateTotalTransform().Apply(CalculatedBounds);

	public abstract void ApplyStyle();

	/// <summary>
	///     Updates this component.
	/// </summary>
	/// <param name="delta"></param>
	public virtual void Update(float delta)
	{
	}

	/// <summary>
	///     Draws this component to the screen.
	/// </summary>
	/// <param name="graphicsProvider"></param>
	public virtual void Draw(IGraphicsProvider graphicsProvider)
	{
		if (Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			{
				DrawRectangles(graphicsProvider);
			}
			graphicsProvider.EndOverflow();
		}

		DrawRectangles(graphicsProvider);
	}

	private void DrawRectangles(IGraphicsProvider graphicsProvider)
	{
		var (left, right, top, bottom) = BorderSize;
		var (topLeft, topRight, bottomLeft, bottomRight) = BorderRadii;

		// Outer (border) rectangle
		graphicsProvider.DrawRect(TransformedBounds, BorderColor,
			topLeft, topRight, bottomLeft, bottomRight);

		// Inner (background color) rectangle
		var innerRectangleBounds = Box2.FromSize(
			TransformedBounds.Location + new Vector2(left, bottom),
			TransformedBounds.Size - new Vector2(left + right, top + bottom)
		);
		graphicsProvider.DrawRect(innerRectangleBounds, BackgroundColor,
			topLeft, topRight, bottomLeft, bottomRight);
	}

	/// <summary>
	///     Calculates the layout of this component. Get the newly calculated bounds with <seealso cref="CalculatedBounds" />.
	/// </summary>
	public void CalculateLayout(float sceneWidth = float.NaN, float sceneHeight = float.NaN)
	{
		Node.CalculateLayout(sceneWidth, sceneHeight);
	}

	public virtual Component? Hit(Vector2 position) => HitsThis(position) ? this : null;

	public bool HitsThis(Vector2 position)
	{
		if (!Visible)
		{
			return false;
		}

		var (pointX, pointY) = position;

		var bounds = TransformedBounds;
		var width = bounds.Width;
		var height = bounds.Height;
		var leftEdge = bounds.X;
		var rightEdge = leftEdge + width;
		var topEdge = bounds.Y;
		var bottomEdge = topEdge + height;

		var (topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius) = BorderRadii;

		// Check if the point is within the rectangular bounds
		if (pointX < leftEdge || pointX > rightEdge || pointY < topEdge || pointY > bottomEdge)
		{
			return false;
		}

		// Check if the point is within the rounded corners

		// Top-left corner
		if (pointX <= leftEdge + topLeftRadius && pointY <= topEdge + topLeftRadius)
		{
			var dx = pointX - (leftEdge + topLeftRadius);
			var dy = pointY - (topEdge + topLeftRadius);
			return dx * dx + dy * dy <= topLeftRadius * topLeftRadius;
		}

		// Top-right corner
		if (pointX >= rightEdge - topRightRadius && pointY <= topEdge + topRightRadius)
		{
			var dx = pointX - (rightEdge - topRightRadius);
			var dy = pointY - (topEdge + topRightRadius);
			return dx * dx + dy * dy <= topRightRadius * topRightRadius;
		}

		// Bottom-left corner
		if (pointX <= leftEdge + bottomLeftRadius && pointY >= bottomEdge - bottomLeftRadius)
		{
			var dx = pointX - (leftEdge + bottomLeftRadius);
			var dy = pointY - (bottomEdge - bottomLeftRadius);
			return dx * dx + dy * dy <= bottomLeftRadius * bottomLeftRadius;
		}

		// Bottom-right corner
		// ReSharper disable once InvertIf
		if (pointX >= rightEdge - bottomRightRadius && pointY >= bottomEdge - bottomRightRadius)
		{
			var dx = pointX - (rightEdge - bottomRightRadius);
			var dy = pointY - (bottomEdge - bottomRightRadius);
			return dx * dx + dy * dy <= bottomRightRadius * bottomRightRadius;
		}

		return true; // The point is inside the rounded rectangle
	}

	/// <summary>
	///     Calculates the total transform of this component, including parent component transforms
	/// </summary>
	/// <returns></returns>
	public Transform CalculateTotalTransform()
	{
		if (Parent != null)
		{
			return Parent.CalculateTotalTransform() + Transform;
		}

		return Transform;
	}
}

public class Transform
{
	public Vector2 Position { get; set; }

	public static Transform operator +(Transform left, Transform right) => new()
	{
		Position = left.Position + right.Position
	};

	public Box2 Apply(Box2 input) => Box2.FromPositions(input.Min + Position, input.Max + Position);
}