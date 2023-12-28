namespace CodeFirst.Gui.Events;

public abstract class UiEvent
{
	public bool DefaultPrevented { get; private set; }

	public bool Cancelled { get; private set; }

	/// <summary>
	///     Prevents this event from triggering the <see cref="Frame" />'s internal handle method. Event handlers
	///     will still be called. To disable subsequent event handlers call <see cref="Cancel" />.
	/// </summary>
	/// <seealso cref="Cancel" />
	/// <seealso cref="DefaultPrevented" />
	public void PreventDefault() => DefaultPrevented = true;

	/// <summary>
	///     Cancels this event, preventing it from reaching other event handlers. The event will still trigger the
	///     <see cref="Frame" />'s internal handle method. To disable the internal handle method, call
	///     <see cref="PreventDefault" />.
	/// </summary>
	/// <seealso cref="PreventDefault" />
	/// <seealso cref="Cancelled" />
	public void Cancel() => Cancelled = true;
}