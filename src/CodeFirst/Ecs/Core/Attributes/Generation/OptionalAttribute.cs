namespace CodeFirst.Ecs.Core.Attributes.Generation;

/// <summary>
///     An attribute that marks the given <see cref="EntityProcessorAttribute" /> method's parameter as optional.
/// </summary>
/// <remarks>
///     Only works when the <c>JXS.Ecs.Generators</c> package is installed. If that package is not installed, this
///     attribute does nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionalAttribute : Attribute
{
}