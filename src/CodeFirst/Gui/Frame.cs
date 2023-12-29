using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public partial class Frame
{
	private readonly List<Frame> children;
	private readonly HashSet<UiAction> pressedActions;

	protected internal readonly YogaNode Node;
	private Scene? scene;

	public Frame()
	{
		children = new List<Frame>();
		pressedActions = new HashSet<UiAction>();
		Node = new YogaNode
		{
			Data = this
		};
		Transform = new Transform();
	}

	public Transform Transform { get; }

	/// <summary>
	///     A shorthand for getting and setting <see cref="Display" />.
	/// </summary>
	public bool Visible
	{
		get => Display != YogaDisplay.None;
		set => Display = value ? YogaDisplay.Flex : YogaDisplay.None;
	}

	/// <summary>
	///     A shorthand for getting and setting <see cref="BorderLeftWidth" />, <see cref="BorderRightWidth" />,
	///     <see cref="BorderTopWidth" />, and <see cref="BorderBottomWidth" />.
	/// </summary>
	public BorderSize BorderSize
	{
		get => new()
		{
			Left = NaNSwitch(BorderLeftWidth, BorderWidth),
			Right = NaNSwitch(BorderRightWidth, BorderWidth),
			Top = NaNSwitch(BorderTopWidth, BorderWidth),
			Bottom = NaNSwitch(BorderBottomWidth, BorderWidth)
		};
		set => (BorderTopWidth, BorderRightWidth, BorderBottomWidth, BorderLeftWidth) = value;
	}

	/// <summary>
	///     A shorthand for getting and setting <see cref="BorderTopLeftRadius" />, <see cref="BorderTopRightRadius" />,
	///     <see cref="BorderBottomLeftRadius" />, and <see cref="BorderBottomRightRadius" />.
	/// </summary>
	public BorderRadii BorderRadii
	{
		get => new()
		{
			TopLeft = NaNSwitch(BorderTopLeftRadius, BorderRadius),
			TopRight = NaNSwitch(BorderTopRightRadius, BorderRadius),
			BottomLeft = NaNSwitch(BorderBottomLeftRadius, BorderRadius),
			BottomRight = NaNSwitch(BorderBottomRightRadius, BorderRadius)
		};
		set => (BorderTopLeftRadius, BorderTopRightRadius, BorderBottomLeftRadius, BorderBottomRightRadius) = value;
	}

	public string? Id { get; init; }

	/// <summary>
	///     The parent of this Component (if any).
	/// </summary>
	public Frame? Parent { get; protected internal set; }

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
			var x = Parent is null ? Node.LayoutX : Node.LayoutX + Parent.CalculatedBounds.X - Parent.ScrollOffset.X;
			var y = Parent is null ? Node.LayoutY : Node.LayoutY + Parent.CalculatedBounds.Y - Parent.ScrollOffset.Y;
			return Box2.FromSize(new Vector2(x, y), new Vector2(Node.LayoutWidth, Node.LayoutHeight));
		}
	}

	/// <summary>
	///     The bounds after applying the transform of this component. Use this for rendering and hit detection etc.
	/// </summary>
	/// <seealso cref="CalculatedBounds" />
	/// <seealso cref="CalculateTotalTransform" />
	public Box2 TransformedBounds => CalculateTotalTransform().Apply(CalculatedBounds);

	protected virtual void OnAddedToScene()
	{
	}

	protected virtual void OnRemovedFromScene()
	{
	}

	/// <summary>
	///     Updates this component.
	/// </summary>
	/// <param name="delta"></param>
	public virtual void Update(float delta)
	{
		foreach (var child in children.Where(c => c.Visible))
		{
			child.Update(delta);
		}
	}

	/// <summary>
	///     Draws this component to the screen.
	/// </summary>
	/// <param name="graphicsProvider"></param>
	public virtual void Draw(IGraphicsProvider graphicsProvider)
	{
		UpdateScrollOffset();

		if (Overflow != YogaOverflow.Visible)
		{
			graphicsProvider.BeginOverflow();
			{
				DrawRectangles();
				DrawChildren();
			}
			graphicsProvider.EndOverflow();
		}
		else
		{
			DrawRectangles();
			DrawChildren();
		}

		void DrawRectangles()
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

			if (Overflow == YogaOverflow.Visible)
			{
				graphicsProvider.DrawRect(innerRectangleBounds, BackgroundColor,
					topLeft, topRight, bottomLeft, bottomRight);
			}
			else
			{
				// We don't want this inner rectangle to affect the stencil buffer
				// StencilMask(0x00);
				graphicsProvider.DrawRect(innerRectangleBounds, BackgroundColor,
					topLeft, topRight, bottomLeft, bottomRight);
				// StencilMask(0xff);
			}
		}

		void DrawChildren()
		{
			foreach (var child in children.Where(c => c.Visible))
			{
				child.Draw(graphicsProvider);
			}
		}
	}

	/// <summary>
	///     Calculates the layout of this component. Get the newly calculated bounds with <seealso cref="CalculatedBounds" />.
	/// </summary>
	public void CalculateLayout(float sceneWidth = float.NaN, float sceneHeight = float.NaN)
	{
		Node.CalculateLayout(sceneWidth, sceneHeight);
	}

	/// <summary>
	///     Calculates the total transform of this component, including parent component transforms
	/// </summary>
	/// <returns></returns>
	public Transform CalculateTotalTransform()
	{
		if (Parent == null)
		{
			return Transform;
		}

		return Parent.CalculateTotalTransform() + Transform;
	}

	private static float NaNSwitch(float value, float fallback) => float.IsNaN(value) ? fallback : value;
}