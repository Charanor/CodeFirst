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
	///     Asynchronously saves the current logs to a file. Does <b>not</b> clear the logs, any subsequent call to this
	///     method will include all logs previously logged.
	/// </summary>
	/// <param name="isCrash">if <c>true</c>,the file is saved as a crash file (<c>"-CRASH"</c> suffix)</param>
	public static async Task<bool> SaveToFile(bool isCrash = false)
	{
		// Create the directory if it doesn't already exist
		Directory.CreateDirectory(LogsDirectory);
		var time = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
		var logFileName = Path.Combine(LogsDirectory, $"{time}{(isCrash ? "-CRASH" : "")}.txt");
		return await WriteToStream(File.OpenWrite(logFileName));
	}

	/// <inheritdoc cref="SaveToFile" />
	public static bool SaveToFileSynchronous(bool isCrash = false) => SaveToFile(isCrash).Result;

	public static async Task<bool> WriteToStream(Stream stream)
	{
		if (!stream.CanWrite)
		{
			return false;
		}

		try
		{
			await using var sw = new StreamWriter(stream);
			await sw.WriteAsync(LogBuilder.ToString());
			await sw.FlushAsync();
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