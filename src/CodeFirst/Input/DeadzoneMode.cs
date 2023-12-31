namespace CodeFirst.Input;

public enum DeadzoneMode
{
	/// <summary>
	///     Normalizes the output value within the deadzone bounds. E.g. if the deadzone is 0.15 then the value range
	///     will still be (0, 1).
	/// </summary>
	Normalize,

	/// <summary>
	///     Clamps the output value to within the deadzone bounds. E.g. if deadzone is 0.15 then the value range will
	///     be (0.15, 1).
	/// </summary>
	Clamp
}