namespace JXS.Ecs.Core;

/// <summary>
///     Represents a collection of component flags indicating if the component should be required
/// </summary>
/// <param name="All">Only matches entities that have <i>all</i> components in this collection.</param>
/// <param name="Some">Only matches entities that have <i>at least one</i> component in this collection.</param>
/// <param name="None">Only matches entities that have <i>no</i> components in this collection.</param>
public record Aspect(ComponentFlags All, ComponentFlags Some, ComponentFlags None) : IAspect
{
	public bool Matches(World world, Entity entity)
	{
		var flags = world.GetFlagsForEntity(entity);
		var containsAll = All.Empty || flags.ContainsAll(All);
		var containsSome = Some.Empty || flags.ContainsSome(Some);
		var containsNone = None.Empty || flags.ContainsNone(None);
		return containsAll && containsSome && containsNone;
	}
}