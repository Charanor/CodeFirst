namespace CodeFirst.Ecs.Core.Attributes.Generation;

/// <summary>
///     An attribute that marks the given method as an entity processing method, allowing the parent
///     <see cref="IteratingSystem" /> to generate a <see cref="IteratingSystem.Update" /> method that automatically
///     calls the entity processing method.
/// </summary>
/// <remarks>
///     Only works when the <c>CodeFirst.Generators</c> package is installed. If that package is not installed, this
///     attribute does nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class EntityProcessorAttribute : Attribute
{
}