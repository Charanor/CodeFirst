namespace CodeFirst.Input;

public interface IKeyboardFocusable
{
	public bool Focused { get; set; }

	public void OnTextTyped(string typedText);
}