using JXS.Utils.Events;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public interface IInputProvider
{
	MouseState MouseState { get; }
	KeyboardState KeyboardState { get; }
	
	event EventHandler<IInputProvider, KeyboardKeyEventArgs>? OnKeyUp;
	event EventHandler<IInputProvider, KeyboardKeyEventArgs>? OnKeyDown;
	event EventHandler<IInputProvider, KeyboardKeyEventArgs>? OnKeyPressed;
	
	event EventHandler<IInputProvider, MouseButtonEventArgs>? OnButtonUp;
	event EventHandler<IInputProvider, MouseButtonEventArgs>? OnButtonDown;
	event EventHandler<IInputProvider, MouseButtonEventArgs>? OnButtonPressed;

	event EventHandler<IInputProvider, MouseMoveEventArgs>? OnMouseMoved; 
	event EventHandler<IInputProvider, MouseWheelEventArgs>? OnMouseWheel;

	event EventHandler<IInputProvider, FileDropEventArgs>? OnFileDrop;

	event EventHandler<IInputProvider, TextInputEventArgs>? OnTextInput;
}