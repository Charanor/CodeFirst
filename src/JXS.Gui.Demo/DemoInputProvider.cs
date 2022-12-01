using OpenTK.Mathematics;

namespace JXS.Gui.Demo;

public class DemoInputProvider : IInputProvider
{
	public IKeyboardFocusable? KeyboardFocus { get; set; }
	public Vector2 MousePosition { get; }

	public bool JustPressed(string action) => false;
	public bool JustReleased(string action) => false;
}