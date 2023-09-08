using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public interface IGuiInputProvider
{
	IKeyboardFocusable? KeyboardFocus { get; set; }
	Vector2 MousePosition { get; }

	bool JustPressed(GuiInputAction action);
	bool JustReleased(GuiInputAction action);

	bool IsPressed(GuiInputAction action);

	event EventHandler<IGuiInputProvider, string>? OnTextInput;
}

public enum GuiInputAction
{
	Primary,
	Secondary
}