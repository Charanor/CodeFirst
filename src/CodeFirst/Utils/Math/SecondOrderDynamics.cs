using System.Numerics;

namespace CodeFirst.Utils.Math;

public sealed class SecondOrderDynamics<T> where T :
	IAdditionOperators<T, T, T>,
	ISubtractionOperators<T, T, T>,
	IDivisionOperators<T, float, T>,
	IMultiplyOperators<T, float, T>,
	IAdditiveIdentity<T, T>
{
	private readonly float w;
	private readonly float z;
	private readonly float d;
	private readonly float k1;
	private readonly float k2;
	private readonly float k3;

	private T previous;
	private T output;
	private T outputVelocity;

	public SecondOrderDynamics(float frequency, float damping, float undershoot, T initial)
	{
		w = frequency * MathF.Tau;
		z = damping;
		d = w * MathF.Sqrt(MathF.Abs(damping * damping - 1));
		k1 = damping / (MathF.PI * frequency);
		k2 = 1f / (w * w);
		k3 = undershoot * damping / w;
		previous = initial;
		output = initial;
		outputVelocity = T.AdditiveIdentity;
	}

	public T Update(T value, float delta)
	{
		var xd = (value - previous) / delta;
		previous = value;
		return Update(value, xd, delta);
	}

	public T Update(T value, T velocity, float delta)
	{
		// Implementation of semi-implicit Euler method
        
		float k1Stable;
		float k2Stable;

		// If the resonant frequency is too high relative to the delta the system becomes unstable (e.g. during lag spikes).
		// Here we calculate values of k1 and k2 so that they always damp the system.
		//
		// The "proper" solution here is to constrain the delta time, and then if delta exceeds this constraint we do
		// several iterations, e.g. if delta = 2 * maxDelta we would do the following integration twice in sequence.
		// However, in reality a too-large delta is caused by a lag spike and the last thing we want to do is add more
		// calculations so we use this alternative method that constrains k1 and k2 instead which gives less accurate
		// results but better performance. It also has the effect of preventing jitter for high frequency values.
		if (w * delta < z)
		{
			k1Stable = k1;
			k2Stable = MathF.Max(k2, MathF.Max(delta * delta / 2 + delta * k1 / 2, delta * k1));
		}
		else
		{
			var t1 = MathF.Exp(-z * w * delta);
			var alpha = 2 * t1 * (z <= 1 ? MathF.Cos(delta * d) : MathF.Cosh(delta * d));
			var beta = t1 * t1;
			var t2 = delta / (1 + beta - alpha);
			k1Stable = (1 - beta) * t2;
			k2Stable = delta * t2;
		}

		output += outputVelocity * delta;
		// From the eq. of motion: y(x) + k1 * y'(x) + k2 * y''(x) = x + k3 * x, solve for y''(x)
		var acceleration = value + velocity * k3 - output - outputVelocity * k1Stable;
		outputVelocity += acceleration * delta / k2Stable;
		return output;
	}
}