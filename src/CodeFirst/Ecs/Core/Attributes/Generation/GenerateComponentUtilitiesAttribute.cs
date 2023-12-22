namespace CodeFirst.Ecs.Core.Attributes.Generation;

/// <summary>
///     A class marked with this attribute will have a partial implementation of that class generated that contains
///     utility methods for the component types passed in the constructor.
/// </summary>
/// <remarks>
///     Only works when the <c>CodeFirst.Generators</c> package is installed. If that package is not installed, this
///     attribute does nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class GenerateComponentUtilitiesAttribute : Attribute
{
	private readonly List<Type> componentTypes;

	public GenerateComponentUtilitiesAttribute(Type component, params Type[] restTypes)
	{
		componentTypes = new List<Type> { component };
		componentTypes.AddRange(restTypes);
	}

	public IReadOnlyList<Type> ComponentTypes => componentTypes;
}