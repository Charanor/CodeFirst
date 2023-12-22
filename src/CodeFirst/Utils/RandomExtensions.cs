using OpenTK.Mathematics;

namespace CodeFirst.Utils;

public static class RandomExtensions
{
	public static T PickRandom<T>(this Random random, params T[] items)
	{
		var idx = random.Next(minValue: 0, items.Length);
		return items[idx];
	}

	public static TEnum PickRandom<TEnum>(this Random random) where TEnum : struct, Enum =>
		PickRandom(random, Enum.GetValues<TEnum>());

	public static Vector2 RandomUnitVector(this Random random)
	{
		var angle = (random.NextSingle() * 2 - 1) * MathF.PI;
		var x = MathF.Cos(angle);
		var y = MathF.Sin(angle);
		return new Vector2(x, y);
	}

	public static float NextSingle(this Random random, float min, float max) => min + random.NextSingle() * (max - min);

	public static bool NextBool(this Random random) => random.NextSingle() <= 0.5f;
}