namespace JXS.Gui.Components;

public class KeyboardUnfocuser : Pressable
{
    public KeyboardUnfocuser(Style? style, string? id, IInputProvider inputProvider) : base(style, id, inputProvider)
    {
        OnFullPress += (_, args) =>
        {
            if (args.PressEvent != PressEvent.Primary) return;
            inputProvider.KeyboardFocus = null;
        };
    }
}