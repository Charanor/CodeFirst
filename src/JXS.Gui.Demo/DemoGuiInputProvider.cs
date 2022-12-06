using JXS.Utils.Events;
using OpenTK.Mathematics;

namespace JXS.Gui.Demo;

public class DemoGuiInputProvider : IGuiInputProvider
{
	public IKeyboardFocusable? KeyboardFocus { get; set; }
	public Vector2 MousePosition { get; }

	public bool JustPressed(GuiInputAction action) => false;
	public bool JustReleased(GuiInputAction action) => false;
	
	public event EventHandler<IGuiInputProvider, string>? OnTextInput;

	public void TextInput(string input) => OnTextInput?.Invoke(this, input);
}