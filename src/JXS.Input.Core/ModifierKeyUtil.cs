using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public static class ModifierKeyUtil
{
    public static bool IsDown(this ModifierKey modifier, KeyboardState state)
    {
        var leftShift = state.IsKeyDown(Keys.LeftShift);
        var rightShift = state.IsKeyDown(Keys.RightShift);
        if (modifier.HasFlag(ModifierKey.LeftShift) && !leftShift)
            return false;
        if (modifier.HasFlag(ModifierKey.RightShift) && !rightShift)
            return false;
        if (modifier.HasFlag(ModifierKey.Shift) && !leftShift && !rightShift)
            return false;

        var leftControl = state.IsKeyDown(Keys.LeftControl);
        var rightControl = state.IsKeyDown(Keys.RightControl);
        if (modifier.HasFlag(ModifierKey.LeftControl) && !leftControl)
            return false;
        if (modifier.HasFlag(ModifierKey.RightControl) && !rightControl)
            return false;
        if (modifier.HasFlag(ModifierKey.Control) && !leftControl && !rightControl)
            return false;

        var leftAlt = state.IsKeyDown(Keys.LeftAlt);
        var rightAlt = state.IsKeyDown(Keys.RightAlt);
        if (modifier.HasFlag(ModifierKey.LeftAlt) && !leftAlt)
            return false;
        if (modifier.HasFlag(ModifierKey.RightAlt) && !rightAlt)
            return false;
        if (modifier.HasFlag(ModifierKey.Alt) && !leftAlt && !rightAlt)
            return false;

        return !modifier.HasFlag(ModifierKey.None) ||
               // No modifier key pressed
               !leftAlt && !rightAlt && !leftControl && !rightControl && !leftShift && !rightShift;
    }
}