using System.Runtime.CompilerServices;
using System.Text;

namespace CodeFirst.Utils.Logging;

public class Logger : ILogger
{
	private const string INDENT_TAIL = "    "; // 4 spaces
	private const string INDENT_HEAD = "  ├─"; // 2 spaces

	protected static readonly string Format = "[{0:HH:mm:ss.fff}] {1,5} in {5}#{6} line {7}: ({3}) {4}{2}\n";
	protected static readonly string FormatWithoutTrace = "[{0:HH:mm:ss.fff}] {1,5}: ({3}) {4}{2}\n";

	private int indentation;

	protected internal Logger(string name)
	{
		Name = name;
	}

	public string Name { get; }

	// private string IndentString => "│    "; // Visual guide + 4 spaces

	public bool EnableDebugTrace { get; set; }
	public bool EnableInfoTrace { get; set; }
	public bool EnableWarnTrace { get; set; }
	public bool EnableErrorTrace { get; set; }
	public bool EnableTraceTrace { get; set; }

	public void Trace(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		if (LoggingManager.LogLevel.HasFlag(LogLevel.Trace))
		{
			Log("Trace", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber,
				EnableTraceTrace);
		}
	}

	public void Debug(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		if (LoggingManager.LogLevel.HasFlag(LogLevel.Debug))
		{
			Log("Debug", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber,
				EnableDebugTrace);
		}
	}

	public void Info(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		if (LoggingManager.LogLevel.HasFlag(LogLevel.Info))
		{
			Log("Info", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber,
				EnableInfoTrace);
		}
	}

	public void Warn(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		if (LoggingManager.LogLevel.HasFlag(LogLevel.Warn))
		{
			Log("Warn", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber,
				EnableWarnTrace);
		}
	}

	public void Error(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
		if (LoggingManager.LogLevel.HasFlag(LogLevel.Error))
		{
			Log("Error", msg, Name, CreateIndentString(), sourceFilePath, memberName, sourceLineNumber,
				EnableErrorTrace,
				isError: true);
		}
	}

	public void Indent()
	{
		indentation++;
	}

	public void Dedent()
	{
		indentation = System.Math.Max(indentation - 1, val2: 0);
	}

	private string CreateIndentString() =>
		indentation > 0 ? $"{Repeat(INDENT_TAIL, indentation - 1)}{INDENT_HEAD}" : string.Empty;

	protected virtual void Log(string prefix, string msg, string name, string indentString, string sourceFile,
		string memberName, int lineNumber, bool addTrace, bool isError = false)
	{
		var format = addTrace
			? string.Format(Format, DateTime.Now, prefix, msg, name, indentString, sourceFile, memberName, lineNumber)
			: string.Format(FormatWithoutTrace, DateTime.Now, prefix, msg, name, indentString);
		LoggingManager.Write(format);

		var textWriter = isError ? Console.Error : Console.Out;
		textWriter.Write(format);
	}

	private static string Repeat(string value, int count) =>
		new StringBuilder(value.Length * count).Insert(index: 0, value, count).ToString();
}