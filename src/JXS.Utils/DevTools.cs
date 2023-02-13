using JXS.Utils.Logging;

namespace JXS.Utils;

public static class DevTools
{
	private static readonly ILogger Logger = LoggingManager.Get(nameof(DevTools));

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise does nothing.
	/// </summary>
	/// <param name="exception"></param>
	/// <exception cref="Exception"></exception>
	public static void Throw<TResponsible>(Exception exception)
	{
		LoggingManager.Get<TResponsible>().Error(exception.Message);
#if DEBUG
		throw exception;
#endif
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise does nothing.
	/// </summary>
	/// <param name="exception"></param>
	/// <exception cref="Exception"></exception>
	public static void ThrowStatic(Exception exception)
	{
		Logger.Error(exception.Message);
#if DEBUG
		throw exception;
#endif
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise returns value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="exception"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TResponsible"></typeparam>
	/// <returns></returns>
	public static TValue DebugReturn<TValue, TResponsible>(TValue value, Exception exception)
	{
		Throw<TResponsible>(exception);
		return value;
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise returns value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="exception"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <returns></returns>
	public static TValue DebugReturnStatic<TValue>(TValue value, Exception exception)
	{
		ThrowStatic(exception);
		return value;
	}

	/// <summary>
	///     Attempts to run the factory method. If the factory method throws AND DEBUG is defined: throws the exception,
	///     otherwise if the factory method throws and DEBUG is not defined, returns the default value.
	///     If the factory method does not throw, simply returns the returned value.
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="defaultValue"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TResponsible"></typeparam>
	/// <returns></returns>
	public static TValue TryReturn<TValue, TResponsible>(Func<TValue> factory, TValue defaultValue)
	{
		try
		{
			return factory();
		}
		catch (Exception e)
		{
			Throw<TResponsible>(e);
			return defaultValue;
		}
	}

	/// <summary>
	///     Attempts to run the factory method. If the factory method throws AND DEBUG is defined: throws the exception,
	///     otherwise if the factory method throws and DEBUG is not defined, returns the default value.
	///     If the factory method does not throw, simply returns the returned value.
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="defaultValue"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <returns></returns>
	public static TValue TryReturnStatic<TValue>(Func<TValue> factory, TValue defaultValue)
	{
		try
		{
			return factory();
		}
		catch (Exception e)
		{
			ThrowStatic(e);
			return defaultValue;
		}
	}
}