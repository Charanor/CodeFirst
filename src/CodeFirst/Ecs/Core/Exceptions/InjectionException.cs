using System.Reflection;

namespace CodeFirst.Ecs.Core.Exceptions;

public class InjectionException : Exception
{
	public InjectionException(FieldInfo info, string? message) : base($"Could not inject field {info}: {message}")
	{
	}
}