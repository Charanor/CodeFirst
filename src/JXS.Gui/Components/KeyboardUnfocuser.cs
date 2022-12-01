namespace JXS.Gui.Components;

public class KeyboardUnfocuser : Pressable
{
	public KeyboardUnfocuser(string? id = default, Style? style = default) : base(id, style)
	{
		OnFullPress += (_, args) =>
		{
			if (args.PressEvent != PressEvent.Primary)
			{
				return;
			}

			InputProvider.KeyboardFocus = null;
		};
	}
}