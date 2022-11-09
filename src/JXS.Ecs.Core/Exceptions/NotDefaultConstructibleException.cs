namespace JXS.Ecs.Core.Exceptions;

/// <summary>
///     An exception indicating that the type T can not be default constructed; i.e. does not have a 0-argument
///     constructor.
/// </summary>
/// <typeparam name="T">The type that can not be default constructed</typeparam>
public class NotDefaultConstructibleException<T> : MissingMethodException
{
	public NotDefaultConstructibleException() : base(
		$"Type {typeof(T)} is not default constructible; give it a 0-argument constructor.")
	{
	}
}