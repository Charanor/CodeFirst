using System.Text;

namespace JXS.Utils.Logging;

public static class LoggingManager
{
	private static readonly IDictionary<string, ILogger> Loggers = new Dictionary<string, ILogger>();
	private static readonly StringBuilder LogBuilder = new();

	public static string LogsDirectory { get; set; } = "Logs";
	public static Func<string, ILogger> LoggerFactory { get; set; } = CreateLogger;

	public static void Write(string text) => LogBuilder.Append(text);

	/// <summary>
	///     Saves the
	/// </summary>
	public static void SaveToFile()
	{
		// Create the directory if it doesn't already exist
		Directory.CreateDirectory(LogsDirectory);
		var time = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
		var logFileName = Path.Combine(LogsDirectory, $"{time}.txt");
		WriteToStream(File.OpenWrite(logFileName));
		File.WriteAllText(logFileName, LogBuilder.ToString());
	}

	public static bool WriteToStream(Stream stream)
	{
		if (!stream.CanWrite)
		{
			return false;
		}

		try
		{
			using var sw = new StreamWriter(stream);
			sw.Write(LogBuilder.ToString());
			sw.Flush();
		}
		catch (Exception e) when (
			e is ObjectDisposedException or NotSupportedException or IOException)
		{
			return false;
		}

		return true;
	}

	public static ILogger Get(string name)
	{
		if (!Loggers.ContainsKey(name))
		{
			Loggers[name] = LoggerFactory(name);
		}

		return Loggers[name];
	}

	public static ILogger Get<T>() => Get(typeof(T).Name);

	private static ILogger CreateLogger(string name) => new Logger(name);
}