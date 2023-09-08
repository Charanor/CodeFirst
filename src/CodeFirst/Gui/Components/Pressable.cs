using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Gui.Components;

public class Pressable : View
{
	private readonly ISet<GuiInputAction> pressedDown;

	public Pressable()
	{
		pressedDown = new HashSet<GuiInputAction>();
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
		Vector2? hitPos = hit is null ? null : mousePos - hit.TransformedBounds.Min;

		foreach (var action in Enum.GetValues<GuiInputAction>())
		{
			if ((InputProvider?.JustPressed(action) ?? false) && hitsThisOrChild)
			{
				OnPressDown?.Invoke(this, new PressArgs(hit, action, hitPos));
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

	public record PressArgs(Component? Component, GuiInputAction PressEvent, Vector2? Position);
}