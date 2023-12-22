using JetBrains.Annotations;
using OpenTK.Mathematics;

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

	[Pure]
	public Vector2 Apply(in Vector2 start, in Vector2 end, [ValueRange(from: 0, to: 1)] float alpha) =>
		start + (end - start) * interpolator(alpha);

	[Pure]
	public Vector3 Apply(in Vector3 start, in Vector3 end, [ValueRange(from: 0, to: 1)] float alpha) =>
		start + (end - start) * interpolator(alpha);

	private static Interpolator PowInterpolator(int power) => value =>
	{
		if (value <= 0.5f)
		{
			return MathF.Pow(value * 2, power) / 2;
		}

		return MathF.Pow((value - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
	};

	public static Vector3 Spring(Vector3 from, Vector3 to, ref Vector3 vel, float smoothTime, float delta)
	{
		var dynamicVel = (dynamic)vel; // We can't ref of a different type, so we need this intermediate variable
		var result = SpringDynamic(from, to, ref dynamicVel, smoothTime, delta);
		vel = dynamicVel;
		return result;
	}

	public static Vector2 Spring(Vector2 from, Vector2 to, ref Vector2 vel, float smoothTime, float delta)
	{
		var dynamicVel = (dynamic)vel; // We can't ref of a different type, so we need this intermediate variable
		var result = SpringDynamic(from, to, ref dynamicVel, smoothTime, delta);
		vel = dynamicVel;
		return result;
	}

	public static float Spring(float from, float to, ref float vel, float smoothTime, float delta)
	{
		var dynamicVel = (dynamic)vel; // We can't ref of a different type, so we need this intermediate variable
		var result = SpringDynamic(from, to, ref dynamicVel, smoothTime, delta);
		vel = dynamicVel;
		return result;
	}

	/// <summary>
	///     This is a bit of a hack. We use <c>dynamic</c> so we don't have to copy+paste this function
	///     implementation over and over again for each type we want to implement. With .NET 6 preview features
	///     we could do this using arithmetic interfaces, but for now this is a fine workaround.
	/// </summary>
	/// <remarks>
	///     Only works when all of the dynamic types are the same underlying type, and they all allow for the
	///     addition (+), subtraction (-), and multiplication (*) operators between their own types AND <c>float</c>.
	/// </remarks>
	public static T Spring<T>(T from, T to, ref T vel, float smoothTime, float delta)
	{
		var dynamicVel = (dynamic)vel; // We can't ref of a different type, so we need this intermediate variable
		return SpringDynamic(from, to, ref dynamicVel, smoothTime, delta);
	}

	/// <summary>
	///     This is a bit of a hack. We use <c>dynamic</c> so we don't have to copy+paste this function
	///     implementation over and over again for each type we want to implement. With .NET 6 preview features
	///     we could do this using arithmetic interfaces, but for now this is a fine workaround.
	/// </summary>
	/// <remarks>
	///     Only works when all of the dynamic types are the same underlying type, and they all allow for the
	///     addition (+), subtraction (-), and multiplication (*) operators between their own types AND <c>float</c>.
	/// </remarks>
	private static dynamic SpringDynamic(dynamic from, dynamic to, ref dynamic vel, float smoothTime, float delta)
	{
		// Game Programming Gems 4: 1.10
		var omega = 2f / MathF.MaxMagnitude(smoothTime, y: 0.0000001f); // So we don't divide by 0
		var x = omega * delta;
		var exp = 1f / (1f + x + 0.48f * MathF.Pow(x, y: 2) + 0.235f * MathF.Pow(x, y: 3));
		var change = from - to;
		var temp = (vel + change * omega) * delta;
		vel = (vel - omega * temp) * exp;
		return to + (change + temp) * exp;
	}
}

public delegate float Interpolator([ValueRange(from: 0, to: 1)] float value);