using JetBrains.Annotations;

namespace JXS.Utils.Math;

public class Interpolation
{
	public static readonly Interpolation Linear = new(
		static value => value);

	public static readonly Interpolation Smooth = new(
		static value => value * value * (3 - 2 * value));

	public static readonly Interpolation Smoother = new(static value =>
		value * value * value * (value * (value * 6 - 15) + 10));

	public static readonly Interpolation Pow2 = new(PowInterpolator(2));
	public static readonly Interpolation Pow3 = new(PowInterpolator(3));
	public static readonly Interpolation Pow4 = new(PowInterpolator(4));

	private readonly Interpolator interpolator;

	public Interpolation(Interpolator interpolator)
	{
		this.interpolator = interpolator;
	}

	[Pure]
	public float Apply(float start, float end, [ValueRange(from: 0, to: 1)] float alpha) =>
		start + (end - start) * interpolator(alpha);

	private static Interpolator PowInterpolator(int power) => value =>
	{
		if (value <= 0.5f)
		{
			return MathF.Pow(value * 2, power) / 2;
		}

		return MathF.Pow((value - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
	};
}

public delegate float Interpolator([ValueRange(from: 0, to: 1)] float value);