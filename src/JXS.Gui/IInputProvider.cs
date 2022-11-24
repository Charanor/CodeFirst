using OpenTK.Mathematics;

namespace JXS.Gui;

public interface IInputProvider
{
	IKeyboardFocusable? KeyboardFocus { get; set; }
	Vector2 MousePosition { get; }
	
	bool JustPressed(string action);
	bool JustReleased(string action);
}