namespace JXS.Utils.Math;

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

	public double Chance
	{
		get => chanceIncrease * (tries + 1);
		init => chanceIncrease = ChanceIncreaseFromProcChance(value);
	}

	private static double ChanceIncreaseFromProcChance(double chance)
	{
		var upperBounds = chance;
		double lowerBounds = 0;
		double middle;
		double chance2 = 1;

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