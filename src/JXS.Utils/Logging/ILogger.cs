namespace JXS.Utils.Logging;

public interface ILogger
{
	string Name { get; }
	string IndentString { get; }

	void Trace(string msg);
	void Debug(string msg);
	void Info(string msg);
	void Warn(string msg);
	void Error(string msg);

	IndentHandle TraceScope(string msg)
	{
		Trace(msg);
		return new IndentHandle(this);
	}

	void Indent();
	void Dedent();
}