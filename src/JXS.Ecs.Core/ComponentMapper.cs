using JXS.Ecs.Core.Exceptions;

namespace JXS.Ecs.Core;

public class ComponentMapper<T> : IComponentMapper where T : IComponent, IEquatable<T>
{
	private const int DEFAULT_ENTITY_COUNT = 256;
	private readonly World world;

	private readonly bool isDefaultConstructible;
	private readonly bool isValueType;

	private T[] components;
	private bool[] hasComponent;

	public ComponentMapper(World world)
	{
		this.world = world;
		components = new T[DEFAULT_ENTITY_COUNT];
		hasComponent = new bool[DEFAULT_ENTITY_COUNT];
		ComponentId = ComponentManager.GetId<T>();
		isDefaultConstructible = typeof(T).GetConstructor(Type.EmptyTypes) != null;
		isValueType = typeof(T).IsValueType;
	}

	public int ComponentId { get; }

	private void EnsureCanContainId(int entityId)
	{
		if (entityId < 0)
		{
			throw new ArgumentException($"{nameof(entityId)} must be >= 0, got {entityId}", nameof(entityId));
		}

		if (entityId >= hasComponent.Length || entityId >= components.Length)
		{
			var longestArrayLen = Math.Max(components.Length, hasComponent.Length);
			var minSize = Math.Max(longestArrayLen, entityId) + 1;
			Resize(minSize * 2);
		}
	}

	private void Resize(int newSize)
	{
		Array.Resize(ref components, newSize);
		Array.Resize(ref hasComponent, newSize);
	}

	#region IComponentMapper

	/// <inheritdoc cref="IComponentMapper.Has" />
	public virtual bool Has(int entity)
	{
		if (entity < 0 || entity >= hasComponent.Length || entity >= components.Length)
		{
			return false;
		}

		return hasComponent[entity];
	}

	/// <inheritdoc cref="IComponentMapper.Get" />
	public virtual ref T Get(int entity)
	{
		if (!Has(entity))
		{
			throw new InvalidOperationException(
				$"Entity {entity} does not contain a component of type {typeof(T).Name}");
		}

		return ref components[entity];
	}

	/// <inheritdoc cref="IComponentMapper.Update" />
	public virtual ref T Update(int entity, in T component)
	{
		if (!Has(entity))
		{
			throw new ArgumentException($"Entity {entity} does not exist in mapper for type {typeof(T).Name}",
				nameof(entity));
		}

		hasComponent[entity] = true;
		components[entity] = component;
		return ref components[entity];
	}

	/// <inheritdoc cref="IComponentMapper.Create" />
	public virtual ref T Create(int entity)
	{
		if (!isDefaultConstructible && !isValueType)
		{
			throw new NotDefaultConstructibleException<T>();
		}

		EnsureCanContainId(entity);
		hasComponent[entity] = true;
		components[entity] = Activator.CreateInstance<T>();
		world.ComponentAdded(entity);
		return ref components[entity];
	}

	/// <inheritdoc cref="IComponentMapper.Add" />
	public virtual ref T Add(int entity, in T component)
	{
		EnsureCanContainId(entity);
		var hadComponentAlready = Has(entity);

		hasComponent[entity] = true;
		components[entity] = component;

		if (!hadComponentAlready)
		{
			world.ComponentAdded(entity);
		}

		return ref components[entity];
	}

	/// <inheritdoc cref="IComponentMapper.Remove" />
	public virtual void Remove(int entity)
	{
		if (!Has(entity))
		{
			return;
		}

		hasComponent[entity] = false;
		world.ComponentRemoved(entity);
	}

	/// <inheritdoc cref="IComponentMapper.Set" />
	public virtual void Set(int entity, bool shouldHave)
	{
		var has = Has(entity);
		if (has == shouldHave)
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
	public virtual ref T AddIfMissing(int entity, in T component)
	{
		if (Has(entity))
		{
			return ref Get(entity);
		}

		return ref Add(entity, in component);
	}

	#region Non-generics

	IComponent IComponentMapper.Create(int entity) => Get(entity);

	IComponent IComponentMapper.Get(int entity) => Get(entity);

	public void Update(int entity, IComponent component) => Update(entity, (T)component);

	public IComponent Add(int entity, IComponent component) => Add(entity, (T)component);

	public void Update(int entity, in IComponent component) => Update(entity, (T)component);

	public IComponent Add(int entity, in IComponent component) => Add(entity, (T)component);

	public IComponent AddIfMissing(int entity, in IComponent component) => AddIfMissing(entity, (T)component);

	#endregion

	#endregion
}