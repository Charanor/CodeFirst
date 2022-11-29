namespace JXS.Utils;

public static class EnumExtensions
{
	public static IEnumerable<T> GetFlags<T>(this T flags) where T : struct, Enum =>
		Enum.GetValues<T>().Where(value => flags.HasFlag(value));
}