namespace CodeFirst.Input;

[Flags]
public enum ModifierKey
{
	None = 0,
	LeftShift = 1 << 0,
	RightShift = 1 << 1,
	Shift = 1 << 2,
	LeftAlt = 1 << 3,
	RightAlt = 1 << 4,
	Alt = 1 << 5,
	LeftControl = 1 << 6,
	RightControl = 1 << 7,
	Control = 1 << 8
}