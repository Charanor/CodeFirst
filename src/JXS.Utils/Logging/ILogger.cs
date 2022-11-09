namespace JXS.Utils.Logging;

public interface ILogger
{
    public string Name { get; }

    void Trace(string msg);
    void Debug(string msg);
    void Info(string msg);
    void Warn(string msg);
    void Error(string msg);
}