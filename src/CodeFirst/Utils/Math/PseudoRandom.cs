using JetBrains.Annotations;

namespace CodeFirst.Utils.Math;

/// <summary>
///		A pseudo-random implementation where each failed attempt will increase the chance of a success on
///		following attempts, resetting when a success is made. This has the effect of making success and failure streaks
///		rare while still allowing the success chance to average out at the given percentage over time. Essentially
///		this changes the proc-curve to something more like a bell-curve.
///
///		For example if the percentage chance is 20% (0.2) then <see cref="Success"/> will return <c>true</c> approximately
///		every five tries while also making it less likely to return <c>true</c> several times in a row, or <c>false</c>
///		after more than five tries.
/// </summary>
/// <remarks>
///		Inspired by how some abilities, such as crit, worked in Warcraft 3.
/// </remarks>
[PublicAPI]
public class PseudoRandom
{
	private readonly Random random;
	private readonly double chanceIncrease;
	private int tries;

	public PseudoRandom(double chance)
	{
		tries = 0;
		Chance = chance;
		random = new Random();
	}

	/// <summary>
	///		Checks if we had a success or not. Note that this is NOT a pure getter and has side-effects
	///		(can be confusing, sorry -author).
	/// </summary>
	public bool Success
	{
		get
		{
			if (random.NextDouble() > Chance)
			{
				tries += 1;
				return false;
			}

			tries = 0;
			return true;
		}
	}

	/// <summary>
	///		Checks what the current percentage chance of <see cref="Success"/> returning <c>true</c> is. This is a pure
	///		getter, unlike <see cref="Success"/> itself.
	/// </summary>
	public double Chance
	{
		[Pure]
		get => chanceIncrease * (tries + 1);
		init => chanceIncrease = ChanceIncreaseFromProcChance(value);
	}

	private static double ChanceIncreaseFromProcChance(double chance)
	{
		var upperBounds = chance;
		var lowerBounds = 0.0;
		var middle = 0.0;
		var chance2 = 1.0;

		while (true)
		{
			middle = (upperBounds + lowerBounds) / 2.0;
			var chance1 = ProcChanceFromChanceIncrease(middle);

			if (System.Math.Abs(chance1 - chance2) <= 0)
			{
				break;
			}

			if (chance1 > chance)
			{
				upperBounds = middle;
			}
			else
			{
				lowerBounds = middle;
			}

			chance2 = chance1;
		}

		return middle;
	}

	private static double ProcChanceFromChanceIncrease(double chanceIncrease)
	{
		double probability = 0;
		double sum = 0;

		var maxFails = System.Math.Ceiling(1 / chanceIncrease);
		for (var i = 0; i < maxFails; i++)
		{
			var pOnN = System.Math.Min(val1: 1, i * chanceIncrease) * (1 - probability);
			probability += pOnN;
			sum += i * pOnN;
		}

		return 1 / sum;
	}
}