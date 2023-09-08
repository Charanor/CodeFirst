using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CodeFirst.Input;

public static class AxisUtils
{
	public static Axis While(this Axis axis, Axis modifierAxis)
		=> new Modifier(axis, modifierAxis);

	public static Axis While(this Axis axis, ModifierKey modifierKey)
		=> new Modifier(axis, modifierKey);

	public static Axis While(this Axis axis, Keys modifierKey)
		=> new Modifier(axis, new KeyboardButton(modifierKey));

	public static Axis While(this Axis axis, Buttons modifierButton)
		=> new Modifier(axis, new MouseButton(modifierButton));

	public static Axis While(this Axis axis, string axisName, InputSystem inputSystem)
		=> new Modifier(axis, axisName, inputSystem);

	public static Axis WhileNot(this Axis axis, Axis modifierAxis)
		=> new Modifier(axis, modifierAxis.Inverted());

	public static Axis WhileNot(this Axis axis, ModifierKey modifierKey)
		=> WhileNot(axis, new ModifierAxis(modifierKey));

	public static Axis WhileNot(this Axis axis, string axisName, InputSystem inputSystem)
		=> new Modifier(axis, new CopyNamedAxis(axisName, inputSystem).Inverted());

	/// <summary>
	///     Creates a new axis that acts as the unary operator "<c>-</c>" on the value returned by <paramref name="axis" />.
	/// </summary>
	/// <param name="axis">the axis to negate</param>
	/// <returns>a new axis</returns>
	/// <example>
	///     var axis = Axis.Create(Keys.W); <br />
	///     // axis.Value = 0.123 <br />
	///     var negated = axis.Negated(); <br />
	///     // negated.Value = -0.123 <br />
	///     var alwaysZero = Axis.Create(Keys.SPACE); <br />
	///     // alwaysZero.Value = 0 <br />
	///     var alsoAlwaysZero = alwaysZero.Negated(); <br />
	///     // alsoAlwaysZero.Value = -0 <br />
	/// </example>
	public static Axis Negated(this Axis axis) => new NegatedAxis(axis);

	/// <summary>
	///     Creates a new axis that returns <c>0</c> when <paramref name="axis" /> would return any non-zero value (1, -1,
	///     0.17276, 57167, etc.), and <c>1</c> when <paramref name="axis" /> would return <c>0</c>.
	/// </summary>
	/// <param name="axis">the axis to invert</param>
	/// <returns>a new axis</returns>
	public static Axis Inverted(this Axis axis) => new InvertedAxis(axis);
}