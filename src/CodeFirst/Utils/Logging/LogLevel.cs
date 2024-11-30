namespace CodeFirst.Utils.Logging;

[Flags]
public enum LogLevel
{
	None = 0,
	Info = 1 << 0,
	Error = 1 << 1,
	Warn = 1 << 2,
	Debug = 1 << 3,
	Trace = 1 << 4,
	All = Info | Error | Warn | Debug | Trace
}