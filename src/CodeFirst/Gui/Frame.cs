using CodeFirst.Graphics.Core.Utils;
using Facebook.Yoga;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public partial class Frame
{
	private readonly List<Frame> children;
	private readonly HashSet<UIAction> pressedActions;

	protected readonly YogaNode Node;
	private Scene? scene;

	public Frame()
	{
		children = new List<Frame>();
		pressedActions = new HashSet<UIAction>();
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
	public Frame? Parent { get; private set; }

	public Scene? Scene
	{
		get => scene ?? Parent?.Scene;
		protected internal set
		{
			if (scene == value)
			{
				return;
			}

			if (scene != null)
			{
				OnRemovedFromScene();
			}

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
				DrawBackground();
				DrawChildren();
			}
			graphicsProvider.EndOverflow();
		}
		else
		{
			DrawBackground();
			DrawChildren();
		}

		void DrawBackground()
		{
			if (!BeforeDrawBackground(graphicsProvider))
			{
				return;
			}

			var totalOpacity = CalculateTotalOpacity().Clamp(min: 0, max: 1);
			if (BackgroundTexture != null)
			{
				// If we have a background texture, only draw the tinted texture, skip drawing background rectangles
				var backgroundColor = BackgroundColor.W == 0 ? Color4.White : BackgroundColor;
				backgroundColor.W *= totalOpacity;
				graphicsProvider.DrawNinePatch(BackgroundTexture, TransformedBounds, backgroundColor);
				AfterDrawBackground(graphicsProvider);
				return;
			}

			var (left, right, top, bottom) = BorderSize;
			var (topLeft, topRight, bottomLeft, bottomRight) = BorderRadii;

			// Outer (border) rectangle
			graphicsProvider.DrawRect(TransformedBounds, BorderColor,
				topLeft, topRight, bottomLeft, bottomRight);

			// Inner (background color) rectangle
			var innerRectBounds = Box2.FromSize(
				TransformedBounds.Location + new Vector2(left, top),
				TransformedBounds.Size - new Vector2(left + right, top + bottom)
			);

			graphicsProvider.DrawRect(innerRectBounds,
				BackgroundColor with { W = BackgroundColor.W * totalOpacity },
				topLeft, topRight, bottomLeft, bottomRight);

			AfterDrawBackground(graphicsProvider);
		}

		void DrawChildren()
		{
			if (!BeforeDrawChildren(graphicsProvider))
			{
				return;
			}

			foreach (var child in children.Where(c => c.Visible))
			{
				child.Draw(graphicsProvider);
			}

			AfterDrawChildren(graphicsProvider);
		}
	}

	protected virtual bool BeforeDrawBackground(IGraphicsProvider graphicsProvider) => true;

	protected virtual void AfterDrawBackground(IGraphicsProvider graphicsProvider)
	{
	}

	protected virtual bool BeforeDrawChildren(IGraphicsProvider graphicsProvider) => true;

	protected virtual void AfterDrawChildren(IGraphicsProvider graphicsProvider)
	{
	}

	/// <summary>
	///     Calculates the layout of this component. Get the newly calculated bounds with <seealso cref="CalculatedBounds" />.
	/// </summary>
	public void CalculateLayout(float sceneWidth = float.NaN, float sceneHeight = float.NaN)
	{
		Node.CalculateLayout(sceneWidth, sceneHeight);
		NotifyLayout();
	}

	private void NotifyLayout()
	{
		if (Node.HasNewLayout)
		{
			OnNewLayout();
			Node.MarkLayoutSeen();
		}

		foreach (var child in children)
		{
			child.NotifyLayout();
		}
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

	protected virtual void OnNewLayout()
	{
	}

	private static float NaNSwitch(float value, float fallback) => float.IsNaN(value) ? fallback : value;

	protected float CalculateTotalOpacity() => Opacity * (Parent?.CalculateTotalOpacity() ?? 1);
}