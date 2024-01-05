using System.Runtime.CompilerServices;
using System.Text;

namespace CodeFirst.Utils.Logging;

public class Logger : ILogger
{
	protected static readonly string Format = "[{0:HH:mm:ss.fff}] {1,5} in {5}#{6} line {7}: {4}({3}) {2}\n";

	private int indentation;

	protected internal Logger(string name)
	{
		Name = name;
	}

	public string Name { get; }

	public string IndentString => "│    "; // Visual guide + 4 spaces

	public void Trace(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		Log("Trace", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber);
	}

	public void Debug(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		Log("Debug", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber);
	}

	public void Info(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		Log("Info", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber);
	}

	public void Warn(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		Log("Warn", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber);
	}

	public void Error(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		Log("Error", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber, isError: true);
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

	protected virtual void Log(string prefix, string msg, string name, string indentString, string sourceFile,
		string memberName, int lineNumber, bool isError = false)
	{
		var format = string.Format(Format, DateTime.Now, prefix, msg, name, indentString, sourceFile, memberName,
			lineNumber);
		LoggingManager.Write(format);

		var textWriter = isError ? Console.Error : Console.Out;
		textWriter.Write(format);
	}

	private static string Repeat(string value, int count) =>
		new StringBuilder(value.Length * count).Insert(index: 0, value, count).ToString();
}