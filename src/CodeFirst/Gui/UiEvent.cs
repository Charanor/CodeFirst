namespace CodeFirst.Gui;

public class UiEvent
{
	public UiEvent(UiAction action)
	{
		Action = action;
	}

	public UiAction Action { get; }

	public bool Handled { get; private set; }
	public bool DefaultPrevented { get; private set; }

	public void MarkHandled() => Handled = true;
	public void PreventDefault() => DefaultPrevented = true;
}