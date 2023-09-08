using System.Runtime.CompilerServices;
using CodeFirst.Utils.Logging;

namespace CodeFirst.Utils;

public static class DevTools
{
	private const string DEFAULT_SOURCE_FILE_PATH = "<none>";
	private const string DEFAULT_SOURCE_MEMBER_NAME = "<none>";
	private const int DEFAULT_SOURCE_LINE_NUMBER = -1;
	
	private static readonly ILogger Logger = LoggingManager.Get(nameof(DevTools));

	public static Func<bool> IsDevMode { get; set; } = () =>
	{
#if DEBUG
		return true;
#endif
		return false;
	};

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise does nothing.
	/// </summary>
	/// <param name="exception"></param>
	/// <exception cref="Exception"></exception>
	public static void Throw<TResponsible>(Exception exception,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		
		LoggingManager.Get<TResponsible>().Error(exception.Message, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber, memberName);
		if (IsDevMode())
		{
			throw exception;
		}
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise does nothing.
	/// </summary>
	/// <param name="throwingClass">the class that is throwing this exception</param>
	/// <param name="exception"></param>
	/// <exception cref="Exception"></exception>
	public static void ThrowStatic(Type throwingClass, Exception exception,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		LoggingManager.Get(throwingClass.Name).Error(exception.Message, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber, memberName);
		if (IsDevMode())
		{
			throw exception;
		}
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise does nothing.
	/// </summary>
	/// <param name="exception"></param>
	/// <exception cref="Exception"></exception>
	public static void ThrowStatic(Exception exception,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		Logger.Error(exception.Message, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber, memberName);
		if (IsDevMode())
		{
			throw exception;
		}
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise returns value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="exception"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TResponsible"></typeparam>
	/// <returns></returns>
	public static TValue DebugReturn<TValue, TResponsible>(TValue value, Exception exception,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		Throw<TResponsible>(exception, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber, memberName);
		return value;
	}

	/// <summary>
	///     Throws the given exception if DEBUG is defined, otherwise returns value.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="exception"></param>
	/// <typeparam name="TValue"></typeparam>
	/// <returns></returns>
	public static TValue DebugReturnStatic<TValue>(TValue value, Exception exception,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		ThrowStatic(exception, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceFilePath, 
			// ReSharper disable once ExplicitCallerInfoArgument
			sourceLineNumber, memberName);
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
	public static TValue TryReturn<TValue, TResponsible>(Func<TValue> factory, TValue defaultValue,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		try
		{
			return factory();
		}
		catch (Exception e)
		{
			Throw<TResponsible>(e, 
				// ReSharper disable once ExplicitCallerInfoArgument
				sourceFilePath, 
				// ReSharper disable once ExplicitCallerInfoArgument
				sourceLineNumber, memberName);
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
	public static TValue TryReturnStatic<TValue>(Func<TValue> factory, TValue defaultValue,
		// ReSharper disable once InvalidXmlDocComment
		[CallerFilePath] string sourceFilePath = DEFAULT_SOURCE_FILE_PATH,
		// ReSharper disable once InvalidXmlDocComment
		[CallerLineNumber] int sourceLineNumber = DEFAULT_SOURCE_LINE_NUMBER,
		// ReSharper disable once InvalidXmlDocComment
		[CallerMemberName] string memberName = DEFAULT_SOURCE_MEMBER_NAME)
	{
		try
		{
			return factory();
		}
		catch (Exception e)
		{
			ThrowStatic(e, 
				// ReSharper disable once ExplicitCallerInfoArgument
				sourceFilePath, 
				// ReSharper disable once ExplicitCallerInfoArgument
				sourceLineNumber, memberName);
			return defaultValue;
		}
	}

	/// <summary>
	///     Returns the given expression as a string
	/// </summary>
	/// <example>
	///     // Returns "5 + 5"<br />
	///     StringifyExpression(5 + 5);<br />
	///     // Returns "value is MyClass { SomeProperty = 5 }"<br />
	///     StringifyExpression(value is MyClass { SomeProperty = 5 });<br />
	/// </example>
	/// <param name="expression"></param>
	/// <param name="expressionRepresentation"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static string StringifyExpression<T>(
		T expression,
		[CallerArgumentExpression("expression")]
		string expressionRepresentation = "<none>")
		=> expressionRepresentation;
}