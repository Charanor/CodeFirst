// using JXS.Graphics.Text.Layout;
// using JXS.Utils.Events;
//
// namespace JXS.Gui.Components;
//
// public class Input : Pressable, IKeyboardFocusable
// {
//     private const char PASTE_CHAR = '\u0016';
//     private const char COPY_CHAR = '\u0003';
//
//     private static readonly TextStyle DefaultTextStyle = new()
//     {
//         Flex = 1,
//         TextBreakStrategy = TextBreakStrategy.Whitespace,
//     };
//
//     private static readonly Style DefaultStyle = new()
//     {
//         Padding = 6,
//     };
//
//     private readonly Text text;
//     private string value = null!;
//
//     public Input(IInputProvider inputProvider, Style? style, TextStyle? textStyle, TextStyle? placeholderStyle, string initialValue = "",
//         string placeholder = "", string? id = null) : base(
//         style is not null ? Style.Merge(DefaultStyle, style) : style, id, inputProvider)
//     {
//         Style = style ?? new Style();
//         textStyle ??= DefaultTextStyle;
//         placeholderStyle ??= textStyle;
//
//         text = new Text(initialValue, textStyle, id: null, inputProvider);
//         OnValueChange += (_, args) =>
//         {
//             var newValue = args.Value.newValue;
//             var usePlaceholder = newValue.Length == 0;
//
//             if (usePlaceholder)
//                 text.Style = placeholderStyle;
//             else if (args.Value.oldValue.Length == 0)
//                 text.Style = textStyle;
//         };
//         Value = initialValue;
//         Placeholder = placeholder;
//
//         OnFullPress += (_, args) =>
//         {
//             if (args.PressEvent != PressEvent.Primary) return;
//             InputProvider.KeyboardFocus = this;
//         };
//
//         base.AddChild(text);
//     }
//
//     public string Value
//     {
//         get => value;
//         set
//         {
//             if (this.value == value) return;
//             var oldValue = this.value ?? "";
//             this.value = value;
//             OnValueChange?.Invoke(this,
//                 new EventArgs<(string newValue, string oldValue)>((newValue: value, oldValue)));
//         }
//     }
//
//     public string Placeholder { get; set; }
//
//     public bool Focused { get; set; }
//
//     public void OnTextTyped(string typedText)
//     {
//         Value += typedText;
//         // Console.WriteLine(typedText);
//         // foreach (var character in typedText)
//         // {
//         //     if (!char.IsControl(character))
//         //     {
//         //         Value += character;
//         //     }
//         //     else
//         //     {
//         //         if (character == PASTE_CHAR)
//         //         {
//         //             Console.WriteLine("Paste char");
//         //             Value += InputManager.Instance.Clipboard;
//         //         }
//         //         else if (Enum.TryParse(typeof(Keys), Convert.ToByte(character).ToString(), out var key))
//         //         {
//         //             Value = key switch
//         //             {
//         //                 // Remove last character
//         //                 Keys.Back when Value.Length > 0 => Value[..^1],
//         //                 _ => Value
//         //             };
//         //         }
//         //     }
//         // }
//     }
//
//     public event EventHandler<Input, EventArgs<(string newValue, string oldValue)>>? OnValueChange;
//
//     public override void Update(float delta)
//     {
//         base.Update(delta);
//         text.Value = Value.Length == 0 && !Focused ? Placeholder : Value;
//     }
//
//     public override void AddChild(Component component) =>
//         throw new InvalidOperationException("Can not add child to Input");
// }