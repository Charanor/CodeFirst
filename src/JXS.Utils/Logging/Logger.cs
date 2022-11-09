namespace JXS.Utils.Logging;

internal class Logger : ILogger
{
    private const string FORMAT = "[{0:HH:mm:ss.fff}] {1,5}: ({3}) {2}\n";

    internal Logger(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Trace(string msg)
    {
        Log("Trace", msg, Name);
    }

    public void Debug(string msg)
    {
        Log("Debug", msg, Name);
    }

    public void Info(string msg)
    {
        Log("Info", msg, Name);
    }

    public void Warn(string msg)
    {
        Log("Warn", msg, Name);
    }

    public void Error(string msg)
    {
        Log("Error", msg, Name, true);
    }

    private static void Log(string prefix, string msg, string name, bool isError = false)
    {
        var format = string.Format(FORMAT, DateTime.Now, prefix, msg, name);
        LoggingManager.Write(format);

        var textWriter = isError ? Console.Error : Console.Out;
        textWriter.Write(format);
    }
}