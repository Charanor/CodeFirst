namespace JXS.Input.Core;

[Flags]
public enum ModifierKey
{
    None = 1 << 0,
    LeftShift = 1 << 1,
    RightShift = 1 << 2,
    Shift = 1 << 3,
    LeftAlt = 1 << 4,
    RightAlt = 1 << 5,
    Alt = 1 << 6,
    LeftControl = 1 << 7,
    RightControl = 1 << 8,
    Control = 1 << 9
}