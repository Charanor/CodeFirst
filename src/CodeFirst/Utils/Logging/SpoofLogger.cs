using System.Runtime.CompilerServices;

namespace CodeFirst.Utils.Logging;

public class SpoofLogger : ILogger
{
	public string Name => "SpoofLogger";
	public string IndentString => string.Empty;
	
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
	}

	public void TraceExpression<T>(string msg, T expression, string expressionRepresentation = ILogger.DefaultExpression,
		string sourceFilePath = ILogger.DefaultSourceFilePath, int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void Debug(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void DebugExpression<T>(string msg, T expression, string expressionRepresentation = ILogger.DefaultExpression,
		string sourceFilePath = ILogger.DefaultSourceFilePath, int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void Info(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void InfoExpression<T>(string msg, T expression, string expressionRepresentation = ILogger.DefaultExpression,
		string sourceFilePath = ILogger.DefaultSourceFilePath, int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void Warn(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void WarnExpression<T>(string msg, T expression, string expressionRepresentation = ILogger.DefaultExpression,
		string sourceFilePath = ILogger.DefaultSourceFilePath, int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void Error(string msg,
		[CallerFilePath] string sourceFilePath = ILogger.DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		[CallerMemberName] string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void ErrorExpression<T>(string msg, T expression, string expressionRepresentation = ILogger.DefaultExpression,
		string sourceFilePath = ILogger.DefaultSourceFilePath, int sourceLineNumber = ILogger.DefaultSourceLineNumber,
		string memberName = ILogger.DefaultSourceMemberName)
	{
	}

	public void Indent()
	{
	}

	public void Dedent()
	{
	}
}