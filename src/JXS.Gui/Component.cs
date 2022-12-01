using Facebook.Yoga;
using JXS.Gui.Components;
using OpenTK.Mathematics;

namespace JXS.Gui;

public abstract class Component
{
	protected internal readonly YogaNode Node;
	private Scene? scene;

	protected Component(string? id = default, Style? style = default)
	{
		Id = id;
		Style = style ?? new Style();
		Node = new YogaNode();
	}

	public string? Id { get; init; }

	protected IInputProvider InputProvider => Scene.InputProvider;

	public bool Visible => Style.Display != YogaDisplay.None;

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
	///     The Style of this component.
	/// </summary>
	public Style Style { get; set; }

	/// <summary>
	///     The bounds (x, y, width, height) of this component as calculated by <seealso cref="CalculateLayout" />.
	/// </summary>
	public Box2 CalculatedBounds
	{
		get
		{
			var x = Parent is null ? Node.LayoutX : Node.LayoutX + Parent.CalculatedBounds.X;
			var y = Parent is null ? Node.LayoutY : Node.LayoutY + Parent.CalculatedBounds.Y;
			return  Box2.FromSize(new Vector2(x, y), new Vector2(Node.LayoutWidth, Node.LayoutHeight));
		}
	}

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
		if (Style.Overflow == YogaOverflow.Hidden)
		{
			// TODO: Do something
		}
		graphicsProvider.DrawRect(CalculatedBounds, Style.BackgroundColor);
	}

	/// <summary>
	///     Calculates the layout of this component. Get the newly calculated bounds with <seealso cref="CalculatedBounds" />.
	/// </summary>
	public void CalculateLayout(float sceneWidth = float.NaN, float sceneHeight = float.NaN)
	{
		Node.CalculateLayout(sceneWidth, sceneHeight);
	}

	public virtual void ApplyStyle()
	{
		ApplyStyle(Style);
	}

	protected void ApplyStyle(Style style)
	{
		Node.PositionType = style.Position;
		Node.Display = style.Display;
		Node.Overflow = style.Overflow;
		Node.Flex = style.Flex;
		Node.FlexDirection = style.FlexDirection;
		Node.JustifyContent = style.JustifyContent;
		Node.AlignContent = style.AlignContent;
		Node.AlignItems = style.AlignItems;
		Node.AlignSelf = style.AlignSelf;
		Node.Margin = style.Margin;
		Node.MarginLeft = style.MarginLeft;
		Node.MarginRight = style.MarginRight;
		Node.MarginTop = style.MarginTop;
		Node.MarginBottom = style.MarginBottom;
		Node.MarginVertical = style.MarginVertical;
		Node.MarginHorizontal = style.MarginHorizontal;
		Node.Padding = style.Padding;
		Node.PaddingLeft = style.PaddingLeft;
		Node.PaddingRight = style.PaddingRight;
		Node.PaddingTop = style.PaddingTop;
		Node.PaddingBottom = style.PaddingBottom;
		Node.PaddingVertical = style.PaddingVertical;
		Node.PaddingHorizontal = style.PaddingHorizontal;
		Node.Width = style.Width;
		Node.Height = style.Height;
		Node.Left = style.Left;
		Node.Right = style.Right;
		Node.Top = style.Top;
		Node.Bottom = style.Bottom;
		Node.MinWidth = style.MinWidth;
		Node.MinHeight = style.MinHeight;
		Node.MaxWidth = style.MaxWidth;
		Node.MaxHeight = style.MaxHeight;
	}

	public virtual Component? Hit(Vector2 position) => HitsThis(position) ? this : null;

	public bool HitsThis(Vector2 position) =>
		Visible && position.X >= CalculatedBounds.X &&
		position.X <= CalculatedBounds.X + CalculatedBounds.Width &&
		position.Y >= CalculatedBounds.Y && position.Y <= CalculatedBounds.Y + CalculatedBounds.Height;
}