namespace JXS.Graphics.Text.Layout;

public enum TextBreakStrategy
{
	/// <summary>
	///     Do not break the text, ever
	/// </summary>
	None,

	/// <summary>
	///     Only break on whitespace characters
	/// </summary>
	Whitespace,

	/// <summary>
	///     A line can break anywhere, even in the middle of words.
	/// </summary>
	Anywhere
}

public static class TextBreakStrategyExtensions
{
	public static bool ShouldLineBreakOn(this TextBreakStrategy strategy, char character) =>
		strategy switch
		{
			TextBreakStrategy.None => false,
			TextBreakStrategy.Anywhere => true,
			TextBreakStrategy.Whitespace => char.IsWhiteSpace(character),
			_ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, message: null)
		};
}