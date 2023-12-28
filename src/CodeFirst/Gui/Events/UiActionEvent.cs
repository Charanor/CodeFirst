namespace CodeFirst.Gui.Events;

public class UiActionEvent : UiEvent
{
	public UiActionEvent(UiAction action)
	{
		Action = action;
	}

	public UiAction Action { get; }
	
	public bool Handled { get; private set; }

	/// <summary>
	///		Marks this event as handled. A handled action will not propagate outside of the UI stack (e.g. to the game).
	/// </summary>
	/// <seealso cref="Handled"/>
	public void MarkHandled() => Handled = true;
}