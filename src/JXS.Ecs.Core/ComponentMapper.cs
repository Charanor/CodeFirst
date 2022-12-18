using System.Diagnostics.CodeAnalysis;
using JXS.Ecs.Core.Exceptions;

namespace JXS.Ecs.Core;

public class ComponentMapper<T> : IComponentMapper where T : IComponent
{
	private const int DEFAULT_ENTITY_COUNT = 256;
	private readonly World world;

	private readonly bool isDefaultConstructible;

	private T[] components;
	private bool[] hasComponent;

	public ComponentMapper(World world)
	{
		this.world = world;
		components = new T[DEFAULT_ENTITY_COUNT];
		hasComponent = new bool[DEFAULT_ENTITY_COUNT];
		ComponentId = ComponentManager.GetId<T>();
		isDefaultConstructible = typeof(T).IsValueType || typeof(T).GetConstructor(Type.EmptyTypes) != null;
	}

	public int ComponentId { get; }

	private void EnsureCanContainEntity(Entity entity)
	{
		if (!entity.IsValid)
		{
			throw new ArgumentException($"{nameof(entity)} must be a valid {nameof(Entity)}, got {entity}",
				nameof(entity));
		}

		var entityId = entity.Id;
		if (entityId < hasComponent.Length && entityId < components.Length)
		{
			return;
		}

		var longestArrayLen = Math.Max(components.Length, hasComponent.Length);
		var minimumRequiredSize = Math.Max(longestArrayLen, entityId) + 1;
		Resize(minimumRequiredSize * 2);
	}

	private void Resize(int newSize)
	{
		Array.Resize(ref components, newSize);
		Array.Resize(ref hasComponent, newSize);
	}

	#region IComponentMapper

	/// <inheritdoc cref="IComponentMapper.Has" />
	public virtual bool Has(Entity entity)
	{
		if (!entity.IsValid || entity.Id >= hasComponent.Length || entity.Id >= components.Length)
		{
			return false;
		}

		return hasComponent[entity.Id];
	}

	/// <inheritdoc cref="IComponentMapper.Get" />
	public virtual ref T Get(Entity entity)
	{
		if (!Has(entity))
		{
			throw new InvalidOperationException(
				$"Entity {entity} does not contain a component of type {typeof(T).Name}");
		}

		return ref components[entity.Id];
	}

	/// <inheritdoc cref="IComponentMapper.Update" />
	public virtual ref T Update(Entity entity, in T component)
	{
		if (!Has(entity))
		{
			throw new ArgumentException($"Entity {entity} does not exist in mapper for type {typeof(T).Name}",
				nameof(entity));
		}

		ref var compRef = ref Get(entity);
		compRef = component;
		return ref compRef;
	}

	/// <inheritdoc cref="IComponentMapper.Create" />
	public virtual ref T Create(Entity entity)
	{
		if (!isDefaultConstructible)
		{
			throw new NotDefaultConstructibleException<T>();
		}

		var newInstance = Activator.CreateInstance<T>();
		if (Has(entity))
		{
			// Note, can't use Update(Entity, TComponent) here :(
			ref var component = ref Get(entity);
			component = newInstance;
			return ref component;
		}

		EnsureCanContainEntity(entity);
		hasComponent[entity.Id] = true;
		components[entity.Id] = newInstance;
		world.ComponentAdded(entity);
		return ref components[entity.Id];
	}

	/// <inheritdoc cref="IComponentMapper.Add" />
	public virtual ref T Add(Entity entity, in T component)
	{
		if (Has(entity))
		{
			return ref Update(entity, in component);
		}

		EnsureCanContainEntity(entity);
		hasComponent[entity.Id] = true;
		components[entity.Id] = component;
		world.ComponentAdded(entity);
		return ref components[entity.Id];
	}

	/// <inheritdoc cref="IComponentMapper.Remove" />
	public virtual void Remove(Entity entity)
	{
		if (!Has(entity))
		{
			return;
		}

		hasComponent[entity.Id] = false;
		world.ComponentRemoved(entity);
	}

	/// <inheritdoc cref="IComponentMapper.Set" />
	public virtual void Set(Entity entity, bool shouldHave)
	{
		if (Has(entity) == shouldHave)
		{
			return;
		}

		if (shouldHave)
		{
			Create(entity);
		}
		else
		{
			Remove(entity);
		}
	}

	/// <inheritdoc cref="IComponentMapper.AddIfMissing" />
	public virtual ref T AddIfMissing(Entity entity, in T component)
	{
		if (Has(entity))
		{
			return ref Get(entity);
		}

		return ref Add(entity, in component);
	}

	#region Non-generics

	IComponent IComponentMapper.Create(Entity entity) => Get(entity);

	IComponent IComponentMapper.Get(Entity entity) => Get(entity);

	public void Update(Entity entity, IComponent component) => Update(entity, (T)component);

	public IComponent Add(Entity entity, IComponent component) => Add(entity, (T)component);

	public void Update(Entity entity, in IComponent component) => Update(entity, (T)component);

	public IComponent Add(Entity entity, in IComponent component) => Add(entity, (T)component);

	public IComponent AddIfMissing(Entity entity, in IComponent component) => AddIfMissing(entity, (T)component);

	#endregion

	#endregion
}