using System.Text;

namespace JXS.Utils.Logging;

public class Logger : ILogger
{
	protected static readonly string Format = "[{0:HH:mm:ss.fff}] {1,5}: {4}({3}) {2}\n";

	private int indentation;

	protected internal Logger(string name)
	{
		Name = name;
	}

	public string Name { get; }

	public string IndentString => "│    "; // Visual guide + 4 spaces

	public void Trace(string msg)
	{
		Log("Trace", msg, Name, CreateIndentString());
	}

	public void Debug(string msg)
	{
		Log("Debug", msg, Name, CreateIndentString());
	}

	public void Info(string msg)
	{
		Log("Info", msg, Name, CreateIndentString());
	}

	public void Warn(string msg)
	{
		Log("Warn", msg, Name, CreateIndentString());
	}

	public void Error(string msg)
	{
		Log("Error", msg, Name, CreateIndentString(), isError: true);
	}

	public void Indent()
	{
		indentation++;
	}

	public void Dedent()
	{
		indentation = System.Math.Max(indentation - 1, val2: 0);
	}

	private string CreateIndentString() => indentation > 0 ? Repeat(IndentString, indentation) : string.Empty;

	protected virtual void Log(string prefix, string msg, string name, string indentString, bool isError = false)
	{
		var format = string.Format(Format, DateTime.Now, prefix, msg, name, indentString);
		LoggingManager.Write(format);

		var textWriter = isError ? Console.Error : Console.Out;
		textWriter.Write(format);
	}

	private static string Repeat(string value, int count) =>
		new StringBuilder(value.Length * count).Insert(index: 0, value, count).ToString();
}