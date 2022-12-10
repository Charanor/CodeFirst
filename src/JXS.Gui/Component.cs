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
		Node = new YogaNode();
	}

	public abstract bool Visible { get; }
	public abstract YogaOverflow Overflow { get; }
	public abstract Color4<Rgba> BackgroundColor { get; }
	public abstract (float left, float right, float top, float bottom) BorderSize { get; }
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
		protected internal set => scene = value;
	}

	/// <summary>
	///     The bounds (x, y, width, height) of this component as calculated by <seealso cref="CalculateLayout" />.
	/// </summary>
	public Box2 CalculatedBounds
	{
		get
		{
			var x = Parent is null ? Node.LayoutX : Node.LayoutX + Parent.CalculatedBounds.X;
			var y = Parent is null ? Node.LayoutY : Node.LayoutY + Parent.CalculatedBounds.Y;
			return Box2.FromSize(new Vector2(x, y), new Vector2(Node.LayoutWidth, Node.LayoutHeight));
		}
	}

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
		var (left, right, top, bottom) = BorderSize;
		if (Overflow == YogaOverflow.Hidden)
		{
			graphicsProvider.BeginOverflow();
			{
				graphicsProvider.DrawRect(CalculatedBounds, BackgroundColor, top, bottom, left, right, BorderColor);
			}
			graphicsProvider.EndOverflow();
		}

		graphicsProvider.DrawRect(CalculatedBounds, BackgroundColor, top, bottom, left, right, BorderColor);
	}

	/// <summary>
	///     Calculates the layout of this component. Get the newly calculated bounds with <seealso cref="CalculatedBounds" />.
	/// </summary>
	public void CalculateLayout(float sceneWidth = float.NaN, float sceneHeight = float.NaN)
	{
		Node.CalculateLayout(sceneWidth, sceneHeight);
	}

	public virtual Component? Hit(Vector2 position) => HitsThis(position) ? this : null;

	public bool HitsThis(Vector2 position) =>
		Visible && position.X >= CalculatedBounds.X &&
		position.X <= CalculatedBounds.X + CalculatedBounds.Width &&
		position.Y >= CalculatedBounds.Y && position.Y <= CalculatedBounds.Y + CalculatedBounds.Height;
}