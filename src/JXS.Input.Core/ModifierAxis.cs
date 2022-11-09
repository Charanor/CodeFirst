namespace JXS.Input.Core;

public record ModifierAxis(ModifierKey Key) : Axis
{
	public override float Value => Key.IsDown(KeyboardState) ? 0 : 1;
}