using JXS.Utils.Events;
using OpenTK.Mathematics;

namespace JXS.Gui.Components;

public class Pressable : View
{
	private readonly ISet<InputAction> pressedDown;

	public Pressable(string? id = default, Style? style = default) : base(id, style)
	{
		pressedDown = new HashSet<InputAction>();
	}

	/// <summary>
	///     Invoked when the user releases their press over this component, regardless of over which component it
	///     was pressed down.
	/// </summary>
	public event EventHandler<Pressable, PressArgs>? OnPressUp;

	/// <summary>
	///     Invoked when the user presses down on this component. It is not guaranteed to be followed by an
	///     OnPressUp event.
	/// </summary>
	public event EventHandler<Pressable, PressArgs>? OnPressDown;

	/// <summary>
	///     Invoked when the user releases their press over this component, after having first pressed down on this
	///     component.
	/// </summary>
	public event EventHandler<Pressable, PressArgs>? OnFullPress;

	public override void Update(float delta)
	{
		base.Update(delta);
		var mousePos = InputProvider?.MousePosition ?? Vector2.Zero;
		var hit = Scene!.Hit(mousePos);
		var hitsThisOrChild = hit is not null && (hit == this || HasChild(hit));
		Vector2? hitPos = hit is null ? null : mousePos - hit.CalculatedBounds.Min;

		foreach (var action in Enum.GetValues<InputAction>())
		{
			if ((InputProvider?.JustPressed(action) ?? false) && hitsThisOrChild)
			{
				OnPressDown?.Invoke(this,
					new PressArgs(hit!, action, hitPos));
				pressedDown.Add(action);
			}

			if (!InputProvider?.JustReleased(action) ?? false)
			{
				continue;
			}

			var hadPressedDownOnThis = pressedDown.Remove(action);
			if (!hitsThisOrChild)
			{
				continue;
			}

			OnPressUp?.Invoke(this, new PressArgs(hit, action, hitPos));
			if (hadPressedDownOnThis)
			{
				OnFullPress?.Invoke(this, new PressArgs(hit, action, hitPos));
			}
		}
	}

	public class PressArgs : EventArgs
	{
		public PressArgs(Component? component, InputAction pressEvent, Vector2? position)
		{
			Component = component;
			PressEvent = pressEvent;
			Position = position;
		}

		/// <summary>
		///     The component that was pressed down on / released over. Might be null.
		/// </summary>
		public Component? Component { get; }

		public InputAction PressEvent { get; }

		/// <summary>
		///     The pressed/released position relative to the origin of <code>Component</code>. Null if <code>Component</code> is
		///     null.
		/// </summary>
		public Vector2? Position { get; }
	}
}