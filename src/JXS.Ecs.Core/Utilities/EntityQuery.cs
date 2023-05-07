using JXS.Utils;
using JXS.Utils.Collections;

namespace JXS.Ecs.Core.Utilities;

/// <summary>
///     Enables arbitrary querying of entities within a <see cref="World" />. Note that this is slower than doing the
///     checks manually and will also use more memory since it has to store the entity filters etc. in memory.
///     It is however very useful for non- performance-critical code or for prototyping.
/// </summary>
public class EntityQuery
{
	private readonly List<EntityPredicate> entityFilters;
	private readonly Thread creationThread;

	private Aspect entityAspect;
	private IReadOnlySnapshotList<Entity>? cachedAspectMatchingEntities;

	public EntityQuery(World world)
	{
		World = world;
		entityFilters = new List<EntityPredicate>();
		entityAspect = new AspectBuilder();
		creationThread = Thread.CurrentThread;
	}

	private EntityQuery(EntityQuery other)
	{
		World = other.World;
		entityFilters = new List<EntityPredicate>();
		entityFilters.AddRange(other.entityFilters);
		entityAspect = new AspectBuilder(other.entityAspect);
		creationThread = Thread.CurrentThread;
	}

	private bool IsExecuting => cachedAspectMatchingEntities?.IsIterating ?? false;

	private Aspect EntityAspect
	{
		get => entityAspect;
		set
		{
			cachedAspectMatchingEntities = null;
			entityAspect = value;
		}
	}

	public World World { get; set; }

	/// <summary>
	///     Copies this query to a new instance. This allows for multiple iteration at the same time, including multiple
	///     threads.
	/// </summary>
	/// <returns>a copy of this entity query that can be used for simultaneous iteration, or on a different thread</returns>
	public EntityQuery Copy() => new(this);

	/// <summary>
	///     Executes this query and returns the matching entities. Note that it is not allowed to iterate over the same
	///     <see cref="EntityQuery" /> multiple times at the same time.
	/// </summary>
	/// <returns></returns>
	public IEnumerable<Entity> Execute()
	{
		if (IsExecuting)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException("Can not re-execute entity query that is mid-iteration."));
			yield break;
		}

		if (Thread.CurrentThread != creationThread)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException(
					"Can not execute entity query on a different thread than it was created on! Use Copy() to copy a new instance to the new thread."));
			yield break;
		}

		cachedAspectMatchingEntities ??= World.GetEntitiesForAspect(EntityAspect);
		using var handle = cachedAspectMatchingEntities.BeginHandle();
		foreach (var entity in handle)
		{
			if (!EntityMatchesFilters(entity))
			{
				continue;
			}

			yield return entity;
		}
	}

	private bool EntityMatchesFilters(Entity entity) => entityFilters.All(entityFilter => entityFilter(World, entity));

	/// <summary>
	///     Checks that some entity passes the given <paramref name="predicate" />.
	/// </summary>
	/// <param name="predicate">the predicate</param>
	/// <returns><c>this</c>, for chaining</returns>
	/// <exception cref="InvalidOperationException">
	///     if this entity query is in the middle of <see cref="Execute" />
	/// </exception>
	/// <seealso cref="ThatMatch{T}" />
	public EntityQuery ThatMatch(EntityPredicate predicate)
	{
		if (IsExecuting)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException("Can not modify entity query that is mid-iteration."));
			return this;
		}

		if (Thread.CurrentThread != creationThread)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException(
					"Can not modify entity query on a different thread than it was created on! Use Copy() to copy a new instance to the new thread."));
			return this;
		}

		entityFilters.Add(predicate);
		return this;
	}

	#region Component filters

	/// <summary>
	///     Checks that entities returned by this query passes the given <paramref name="predicate" />. This will
	///     implicitly call <see cref="WithAllComponents" /> on the required component types.
	/// </summary>
	/// <param name="predicate">the predicate</param>
	/// <exception cref="InvalidOperationException">
	///     if this entity query is in the middle of <see cref="Execute" />
	/// </exception>
	/// <returns><c>this</c>, for chaining</returns>
	public EntityQuery ThatMatch<TComponent>(ComponentPredicate<TComponent> predicate) where TComponent : IComponent
	{
		var mapper = World.GetMapper<TComponent>();
		return WithAllComponents<TComponent>().ThatMatch((_, entity) => predicate(in mapper.Get(entity)));
	}

	/// <inheritdoc cref="ThatMatch{T}" />
	public EntityQuery ThatMatch<TComponent1, TComponent2>(ComponentPredicate<TComponent1, TComponent2> predicate)
		where TComponent1 : IComponent
		where TComponent2 : IComponent
	{
		var mapper1 = World.GetMapper<TComponent1>();
		var mapper2 = World.GetMapper<TComponent2>();
		return WithAllComponents<TComponent1, TComponent2>()
			.ThatMatch((_, entity) => predicate(in mapper1.Get(entity), in mapper2.Get(entity)));
	}

	/// <inheritdoc cref="ThatMatch{T}" />
	public EntityQuery ThatMatch<TComponent1, TComponent2, TComponent3>(
		ComponentPredicate<TComponent1, TComponent2, TComponent3> predicate)
		where TComponent1 : IComponent
		where TComponent2 : IComponent
		where TComponent3 : IComponent
	{
		var mapper1 = World.GetMapper<TComponent1>();
		var mapper2 = World.GetMapper<TComponent2>();
		var mapper3 = World.GetMapper<TComponent3>();
		return WithAllComponents<TComponent1, TComponent2, TComponent3>().ThatMatch((_, entity) =>
			predicate(in mapper1.Get(entity), in mapper2.Get(entity), in mapper3.Get(entity)));
	}

	/// <inheritdoc cref="ThatMatch{T}" />
	public EntityQuery ThatMatch<TComponent1, TComponent2, TComponent3, TComponent4>(
		ComponentPredicate<TComponent1, TComponent2, TComponent3, TComponent4> predicate)
		where TComponent1 : IComponent
		where TComponent2 : IComponent
		where TComponent3 : IComponent
		where TComponent4 : IComponent
	{
		var mapper1 = World.GetMapper<TComponent1>();
		var mapper2 = World.GetMapper<TComponent2>();
		var mapper3 = World.GetMapper<TComponent3>();
		var mapper4 = World.GetMapper<TComponent4>();
		return WithAllComponents<TComponent1, TComponent2, TComponent3, TComponent4>().ThatMatch((_, entity) =>
			predicate(in mapper1.Get(entity), in mapper2.Get(entity), in mapper3.Get(entity), in mapper4.Get(entity)));
	}

	/// <inheritdoc cref="ThatMatch{T}" />
	public EntityQuery ThatMatch<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(
		ComponentPredicate<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5> predicate)
		where TComponent1 : IComponent
		where TComponent2 : IComponent
		where TComponent3 : IComponent
		where TComponent4 : IComponent
		where TComponent5 : IComponent
	{
		var mapper1 = World.GetMapper<TComponent1>();
		var mapper2 = World.GetMapper<TComponent2>();
		var mapper3 = World.GetMapper<TComponent3>();
		var mapper4 = World.GetMapper<TComponent4>();
		var mapper5 = World.GetMapper<TComponent5>();
		return WithAllComponents<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>().ThatMatch(
			(_, entity) => predicate(in mapper1.Get(entity), in mapper2.Get(entity), in mapper3.Get(entity),
				in mapper4.Get(entity), in mapper5.Get(entity)));
	}

	#endregion

	#region Aspect Queries

	public EntityQuery WithAllComponents<T>() where T : IComponent => WithAllComponents(typeof(T));

	public EntityQuery WithAllComponents<T1, T2>() where T1 : IComponent where T2 : IComponent =>
		WithAllComponents(typeof(T1), typeof(T2));

	public EntityQuery WithAllComponents<T1, T2, T3>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		WithAllComponents(typeof(T1), typeof(T2), typeof(T3));

	public EntityQuery WithAllComponents<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		WithAllComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public EntityQuery WithAllComponents<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		WithAllComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public EntityQuery WithAllComponents(params Type[] componentTypes)
	{
		if (IsExecuting)
		{
			throw new InvalidOperationException("Can not modify entity query that is mid-iteration.");
		}

		EntityAspect = new AspectBuilder(EntityAspect).All(componentTypes);

		return this;
	}

	public EntityQuery WithNoneComponents<T>() where T : IComponent => WithNoneComponents(typeof(T));

	public EntityQuery WithNoneComponents<T1, T2>() where T1 : IComponent where T2 : IComponent =>
		WithNoneComponents(typeof(T1), typeof(T2));

	public EntityQuery WithNoneComponents<T1, T2, T3>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		WithNoneComponents(typeof(T1), typeof(T2), typeof(T3));

	public EntityQuery WithNoneComponents<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		WithNoneComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public EntityQuery WithNoneComponents<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		WithNoneComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public EntityQuery WithNoneComponents(params Type[] componentTypes)
	{
		if (IsExecuting)
		{
			throw new InvalidOperationException("Can not modify entity query that is mid-iteration.");
		}

		EntityAspect = new AspectBuilder(EntityAspect).None(componentTypes);

		return this;
	}

	public EntityQuery WithSomeComponents<T>() where T : IComponent => WithSomeComponents(typeof(T));

	public EntityQuery WithSomeComponents<T1, T2>() where T1 : IComponent where T2 : IComponent =>
		WithSomeComponents(typeof(T1), typeof(T2));

	public EntityQuery WithSomeComponents<T1, T2, T3>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		WithSomeComponents(typeof(T1), typeof(T2), typeof(T3));

	public EntityQuery WithSomeComponents<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		WithSomeComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public EntityQuery WithSomeComponents<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		WithSomeComponents(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public EntityQuery WithSomeComponents(params Type[] componentTypes)
	{
		if (IsExecuting)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException("Can not modify entity query that is mid-iteration."));
			return this;
		}

		if (Thread.CurrentThread != creationThread)
		{
			DevTools.Throw<EntityQuery>(
				new InvalidOperationException(
					"Can not modify entity query on a different thread than it was created on! Use Copy() to copy a new instance to the new thread."));
			return this;
		}

		EntityAspect = new AspectBuilder(EntityAspect).Some(componentTypes);

		return this;
	}

	#endregion
}

public delegate bool EntityPredicate(World world, Entity entity);

public delegate bool ComponentPredicate<TComponent>(in TComponent component) where TComponent : IComponent;

public delegate bool ComponentPredicate<TComponent1, TComponent2>(
	in TComponent1 component1,
	in TComponent2 component2)
	where TComponent1 : IComponent
	where TComponent2 : IComponent;

public delegate bool ComponentPredicate<TComponent1, TComponent2, TComponent3>(
	in TComponent1 component1,
	in TComponent2 component2,
	in TComponent3 component3)
	where TComponent1 : IComponent
	where TComponent2 : IComponent
	where TComponent3 : IComponent;

public delegate bool ComponentPredicate<TComponent1, TComponent2, TComponent3, TComponent4>(
	in TComponent1 component1,
	in TComponent2 component2,
	in TComponent3 component3,
	in TComponent4 component4)
	where TComponent1 : IComponent
	where TComponent2 : IComponent
	where TComponent3 : IComponent
	where TComponent4 : IComponent;

public delegate bool ComponentPredicate<TComponent1, TComponent2, TComponent3, TComponent4, TComponent5>(
	in TComponent1 component1,
	in TComponent2 component2,
	in TComponent3 component3,
	in TComponent4 component4,
	in TComponent5 component5)
	where TComponent1 : IComponent
	where TComponent2 : IComponent
	where TComponent3 : IComponent
	where TComponent4 : IComponent
	where TComponent5 : IComponent;