namespace CodeFirst.Ecs.Core;

/// <summary>
///     Represents a collection of component flags indicating if the component should be required
/// </summary>
/// <param name="All">Only matches entities that have <i>all</i> components in this collection.</param>
/// <param name="Some">Only matches entities that have <i>at least one</i> component in this collection.</param>
/// <param name="None">Only matches entities that have <i>no</i> components in this collection.</param>
public record Aspect(ComponentFlags All, ComponentFlags Some, ComponentFlags None) : IAspect
{
	public bool IsEmpty => All.IsEmpty && Some.IsEmpty && None.IsEmpty;

	public bool Matches(World world, Entity entity) => Matches(world.GetFlagsForEntityInternal(entity));

	public bool Matches(ComponentFlags flags)
	{
		var containsAll = All.IsEmpty || flags.ContainsAll(All);
		var containsSome = Some.IsEmpty || flags.ContainsSome(Some);
		var containsNone = None.IsEmpty || flags.ContainsNone(None);
		return containsAll && containsSome && containsNone;
	}

	public static Aspect operator &(Aspect left, Aspect right) => new(
		left.All & right.All,
		left.Some & right.Some,
		left.None & right.None
	);

	public static Aspect operator |(Aspect left, Aspect right) => new(
		left.All | right.All,
		left.Some | right.Some,
		left.None | right.None
	);
}