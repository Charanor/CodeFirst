using OpenTK.Mathematics;

namespace JXS.Gui.Demo;

public class DemoInputProvider : IInputProvider
{
	public IKeyboardFocusable? KeyboardFocus { get; set; }
	public Vector2 MousePosition { get; }

	public bool JustPressed(InputAction action) => false;
	public bool JustReleased(InputAction action) => false;
}