using OpenTK.Mathematics;

namespace JXS.Gui;

public interface IInputProvider
{
	IKeyboardFocusable? KeyboardFocus { get; set; }
	Vector2 MousePosition { get; }
	
	bool JustPressed(InputAction action);
	bool JustReleased(InputAction action);
}

public enum InputAction
{
	Primary,
	Secondary
}