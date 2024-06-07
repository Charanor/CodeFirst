using System.Runtime.CompilerServices;

namespace CodeFirst.Utils.Logging;

/// <summary>
///		Create new instances using <see cref="LoggingManager"/>'s <see cref="LoggingManager.Get"/> or <see cref="LoggingManager.Get{T}"/>
/// </summary>
public interface ILogger
{
	protected const string DefaultExpression = "<none>";
	protected const string DefaultSourceFilePath = "<none>";
	protected const string DefaultSourceMemberName = "<none>";
	protected const int DefaultSourceLineNumber = -1;

	string Name { get; }
	
	bool EnableDebugTrace { get; set; }
	bool EnableInfoTrace { get; set; }
	bool EnableWarnTrace { get; set; }
	bool EnableErrorTrace { get; set; }
	bool EnableTraceTrace { get; set; }

	void Trace(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName);

	void Debug(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName);

	void Info(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName);

	void Warn(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName);

	void Error(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName);

	IndentHandle TraceScope(string msg,
		[CallerFilePath] string sourceFilePath = DefaultSourceFilePath,
		[CallerLineNumber] int sourceLineNumber = DefaultSourceLineNumber,
		[CallerMemberName] string memberName = DefaultSourceMemberName)
	{
		Trace(msg,
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath,
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber,
			// ReSharper disable once ExplicitCallerInfoArgument
			memberName);
		return new IndentHandle(this);
	}

	void Indent();
	void Dedent();
}