using CodeFirst.Graphics.Text;
using CodeFirst.Graphics.Text.Layout;
using CodeFirst.Utils.Events;

namespace CodeFirst.Gui.Components;

public class Input : Pressable, IKeyboardFocusable
{
	private const char PASTE_CHAR = '\u0016';
	private const char COPY_CHAR = '\u0003';

	private static readonly TextStyle DefaultTextStyle = new()
	{
		Flex = 1,
		TextBreakStrategy = TextBreakStrategy.Whitespace
	};

	private static readonly Style DefaultStyle = new()
	{
		Padding = 6
	};

	private readonly Text text;
	private string textContent = null!;
	private TextStyle textStyle = new();
	private TextStyle textPlaceholderStyle = new();

	public Input(Font font, string initialTextContent = "", string placeholder = "")
	{
		text = new Text(font, initialTextContent); // Text must be initialised before styles, otherwise NullRefEx

		Style = DefaultStyle;
		TextStyle = DefaultTextStyle;
		TextPlaceholderStyle = DefaultTextStyle;

		TextContent = initialTextContent;
		Placeholder = placeholder;

		OnValueChange += (_, args) =>
		{
			var newValue = args.NewValue;
			var usePlaceholder = newValue.Length == 0;
			var wasUsingPlaceholder = args.OldValue.Length == 0;

			if (usePlaceholder)
			{
				text.Style = TextPlaceholderStyle;
			}
			else if (wasUsingPlaceholder)
			{
				text.Style = TextStyle;
			}
		};

		OnFullPress += (_, args) =>
		{
			if (args.PressEvent != GuiInputAction.Primary)
			{
				return;
			}

			if (InputProvider != null)
			{
				InputProvider.KeyboardFocus = this;
			}
		};

		base.AddChild(text);
	}

	public TextStyle TextStyle
	{
		get => textStyle;
		set
		{
			textStyle = value;
			if (TextContent.Length != 0)
			{
				text.Style = textStyle;
			}
		}
	}

	public TextStyle TextPlaceholderStyle
	{
		get => textPlaceholderStyle;
		set
		{
			textPlaceholderStyle = value;
			if (TextContent.Length == 0)
			{
				text.Style = textPlaceholderStyle;
			}
		}
	}

	public string TextContent
	{
		get => textContent;
		set
		{
			if (textContent == value)
			{
				return;
			}

			var oldValue = textContent;
			textContent = value;
			OnValueChange?.Invoke(this, new TextChange(textContent, oldValue));
		}
	}

	public string Placeholder { get; set; }

	public bool Focused { get; set; }

	public void OnTextTyped(string typedText)
	{
		TextContent += typedText;
		// Console.WriteLine(typedText);
		// foreach (var character in typedText)
		// {
		//     if (!char.IsControl(character))
		//     {
		//         Value += character;
		//     }
		//     else
		//     {
		//         if (character == PASTE_CHAR)
		//         {
		//             Console.WriteLine("Paste char");
		//             Value += InputManager.Instance.Clipboard;
		//         }
		//         else if (Enum.TryParse(typeof(Keys), Convert.ToByte(character).ToString(), out var key))
		//         {
		//             Value = key switch
		//             {
		//                 // Remove last character
		//                 Keys.Back when Value.Length > 0 => Value[..^1],
		//                 _ => Value
		//             };
		//         }
		//     }
		// }
	}

	public event EventHandler<Input, TextChange>? OnValueChange;

	public override void Update(float delta)
	{
		base.Update(delta);
		text.TextContent = TextContent.Length == 0 && !Focused ? Placeholder : TextContent;
	}

	public override void AddChild(Component component) =>
		throw new InvalidOperationException("Can not add child to Input");

	public record TextChange(string NewValue, string OldValue);
}