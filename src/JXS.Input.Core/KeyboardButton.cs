using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public record KeyboardButton : Axis
{
	private readonly Keys key;
	private readonly ModifierKey modifier;

	public KeyboardButton(Keys key, ModifierKey modifier = ModifierKey.None)
	{
		this.key = key;
		this.modifier = modifier;
	}

	public override float Value =>
		KeyboardState.IsKeyDown(key) && modifier.IsDown(KeyboardState) ? 1 : 0;
}