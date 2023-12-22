using CodeFirst.Ecs.Core;

namespace CodeFirst.Ecs.Utils;

/// <summary>
///     A utility class that enables an OOP way of handling components on an entity. Considerably slower than using
///     <see cref="ComponentMapper{T}" /> directly but very useful for prototyping and in contexts where performance
///     is not so important (e.g. initial entity creation on game start).
/// </summary>
public class EntityBuilder
{
	/// <summary>
	///     Constructs a new EntityBuilder linked to the given <see cref="World" />.
	/// </summary>
	/// <param name="world">the world</param>
	/// <param name="entity">
	///     [optional] the entity to edit. If <c>default</c> (or not specified) the entity builder will
	///     automatically create a new entity.
	/// </param>
	public EntityBuilder(World world, Entity entity = default)
	{
		World = world;
		Entity = entity.IsValid ? entity : world.CreateEntity();
	}

	/// <summary>
	///     The world this EntityBuilder is registered on.
	/// </summary>
	public World World { get; }

	/// <summary>
	///     The entity connected to this builder.
	/// </summary>
	public Entity Entity { get; }

	public T Create<T>() where T : IComponent, new() => Add(new T());

	public ref T Add<T>(in T component) where T : IComponent =>
		ref World.GetMapper<T>().Add(Entity, in component);

	public ref T Get<T>() where T : IComponent =>
		ref World.GetMapper<T>().Get(Entity);

	public bool Has<T>() where T : IComponent => World.GetMapper<T>().Has(Entity);

	public bool Remove<T>() where T : IComponent
	{
		if (!Has<T>())
		{
			return false;
		}

		World.GetMapper<T>().Remove(Entity);
		return true;
	}

	public static implicit operator Entity(EntityBuilder builder) => builder.Entity;
}