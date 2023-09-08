namespace CodeFirst.Gui.Components;

public class KeyboardUnfocuser : Pressable
{
	public KeyboardUnfocuser()
	{
		OnFullPress += (_, args) =>
		{
			if (args.PressEvent != GuiInputAction.Primary)
			{
				return;
			}

			if (InputProvider != null)
			{
				InputProvider.KeyboardFocus = null;
			}
		};
	}
}