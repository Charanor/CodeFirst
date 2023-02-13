namespace JXS.Utils.Logging;

public class SpoofLogger : ILogger
{
	public string Name => "SpoofLogger";
	public string IndentString => string.Empty;

	public void Trace(string msg)
	{
	}

	public void Debug(string msg)
	{
	}

	public void Info(string msg)
	{
	}

	public void Warn(string msg)
	{
	}

	public void Error(string msg)
	{
	}

	public void Indent()
	{
	}

	public void Dedent()
	{
	}
}