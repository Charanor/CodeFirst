using System.Reflection;
using JXS.Ecs.Core.Exceptions;
using JXS.Ecs.Core.Utilities;
using JXS.Utils.Collections;

namespace JXS.Ecs.Core;

/// <summary>
///     The core class of the ECS. Handles updating all systems and managing the components of entities.
/// </summary>
public class World
{
	private readonly ISet<Entity> entities;
	private readonly ISet<Entity> removedEntities;

	private readonly IList<EntitySystem> updateSystems;
	private readonly IList<EntitySystem> drawSystems;
	private readonly ISet<Entity> dirtyEntities;

	private readonly IDictionary<Type, IComponentMapper> mappers;
	private readonly IDictionary<Entity, ComponentFlags> entityFlags;

	private readonly IDictionary<Aspect, SnapshotList<Entity>> entitiesForAspects;

	private int nextEntityId;

	public World()
	{
		entities = new HashSet<Entity>();
		removedEntities = new HashSet<Entity>();

		updateSystems = new List<EntitySystem>();
		drawSystems = new List<EntitySystem>();
		dirtyEntities = new HashSet<Entity>();

		mappers = new Dictionary<Type, IComponentMapper>();
		entityFlags = new Dictionary<Entity, ComponentFlags>();

		entitiesForAspects = new Dictionary<Aspect, SnapshotList<Entity>>();
	}

	/// <summary>
	///     Draws all Systems with pass <c>Pass.Update</c>.
	/// </summary>
	/// <param name="delta">time since last Update()</param>
	public void Update(float delta)
	{
		foreach (var system in updateSystems)
		{
			if (!system.ShouldUpdate())
			{
				continue;
			}

			UpdateDirtyEntities();
			system.Begin();
			system.Update(delta);
			system.End();
		}

		RemoveDeletedEntities();
	}

	/// <summary>
	///     Draws all Systems with pass <c>Pass.Draw</c>.
	/// </summary>
	/// <param name="delta">time since last Draw()</param>
	public void Draw(float delta)
	{
		foreach (var system in drawSystems)
		{
			if (!system.ShouldUpdate())
			{
				continue;
			}

			UpdateDirtyEntities();
			system.Begin();
			system.Update(delta);
			system.End();
		}

		RemoveDeletedEntities();
	}

	private void UpdateDirtyEntities()
	{
		foreach (var dirtyEntityId in dirtyEntities)
		{
			HydrateEntity(dirtyEntityId);
		}
	}

	private void RemoveDeletedEntities()
	{
		foreach (var removedEntity in removedEntities)
		{
			entities.Remove(removedEntity);
			foreach (var (_, aspectEntities) in entitiesForAspects)
			{
				if (aspectEntities.Contains(removedEntity))
				{
					aspectEntities.Remove(removedEntity);
				}
			}

			entityFlags.Remove(removedEntity);
			dirtyEntities.Remove(removedEntity);
		}

		removedEntities.Clear();
	}

	/// <summary>
	///     Creates a new entity and adds it to the world.
	/// </summary>
	/// <returns>the id of the created entity</returns>
	/// <exception cref="IndexOutOfRangeException">If we have ran out of new entity ID:s (<c>int.MaxValue = 2147483647</c>)</exception>
	public Entity CreateEntity()
	{
		if (nextEntityId == int.MaxValue)
		{
			throw new IndexOutOfRangeException($"Max number of entities reached, {int.MaxValue}");
		}

		var entity = new Entity(nextEntityId++);
		entities.Add(entity);
		MarkEntityDirty(entity);
		return entity;
	}

	/// <summary>
	///     Marks this entity to be deleted from this <c>World</c> at the end of the current processing stage (update or
	///     draw).
	/// </summary>
	/// <param name="entity">the entity</param>
	public void DeleteEntity(Entity entity) => removedEntities.Add(entity);

	/// <summary>
	///     Adds a system to be processed by this world.
	/// </summary>
	/// <param name="system">the system to add</param>
	/// <exception cref="ArgumentException">If <paramref name="system" /> has already been added to a world before</exception>
	/// <exception cref="InvalidOperationException">If a system of the same type already exists</exception>
	/// <exception cref="InjectionException">If the system contains an injectable field that is not 'readonly'</exception>
	public void AddSystem(EntitySystem system)
	{
		if (system.World is not null)
		{
			throw new ArgumentException(
				$"EntitySystem {system.GetType().Name} has already been added to a world before, do not re-use systems, instead create a new system of the same type.",
				nameof(system));
		}

		switch (system.Pass)
		{
			case Pass.Update:
				if (updateSystems.Any(sys => sys.GetType() == system.GetType()))
				{
					throw new InvalidOperationException(
						$"A system of type {system.GetType()} already exists in world.");
				}

				updateSystems.Add(system);
				break;
			case Pass.Draw:
				if (drawSystems.Any(sys => sys.GetType() == system.GetType()))
				{
					throw new InvalidOperationException(
						$"A system of type {system.GetType()} already exists in world.");
				}

				drawSystems.Add(system);
				break;
			default:
				throw new InvalidOperationException($"Invalid {nameof(system.Pass)} {system.Pass}");
		}

		Inject(system);
		system.World = this;
		system.Entities = GetEntitiesForAspect(system.Aspect);
		system.Initialize(this);
	}

	/// <summary>
	///     Injects any injectable dependencies registered with this world into the given object,
	///     for example component mappers. Fields that receive injected dependencies must be marked as 'readonly'.
	///     <code>
	/// 		// If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///  	private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	///   </code>
	/// </summary>
	/// <param name="obj">The object to inject the dependencies into</param>
	/// <exception cref="InjectionException">If the field definition of the injected dependency is not 'readonly'</exception>
	/// <example>
	///     // If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///     private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	/// </example>
	/// <seealso cref="InjectComponentMappers" />
	public void Inject(object obj)
	{
		InjectComponentMappers(obj);
	}

	/// <summary>
	///     Injects any component defined in given object. The component mapper fields must be <c>readonly</c> and can have
	///     any access modifier (public, private, internal, etc.).
	///     <code>
	/// 		// If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///  	private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	///   </code>
	/// </summary>
	/// <param name="obj">the object to inject the mappers into</param>
	/// <exception cref="InjectionException">If the component mapper field definition is not 'readonly'</exception>
	/// <example>
	///     // If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///     private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	/// </example>
	/// <seealso cref="Inject" />
	public void InjectComponentMappers(object obj)
	{
		var type = obj.GetType();
		foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
		{
			var mapperType = fieldInfo.FieldType;
			if (!mapperType.IsAssignableTo(typeof(IComponentMapper)))
			{
				continue;
			}

			if (!fieldInfo.IsInitOnly)
			{
#if DEBUG
				throw new InjectionException(fieldInfo, message: "Field is not 'readonly'");
#else
				return;
#endif
			}

			fieldInfo.SetValue(obj, GetMapper(mapperType.GenericTypeArguments[0]));
		}
	}

	/// <summary>
	///     Removes a system from this world.
	/// </summary>
	/// <remarks>If the system doesn't exist, nothing happens.</remarks>
	/// <param name="system">the system to remove</param>
	public void RemoveSystem(EntitySystem system)
	{
		updateSystems.Remove(system);
		drawSystems.Remove(system);
	}

	/// <summary>
	///     Get the component mapper for the given component.
	/// </summary>
	/// <typeparam name="T">The type of component</typeparam>
	/// <returns>the component mapper for the component type T</returns>
	public ComponentMapper<T> GetMapper<T>() where T : IComponent =>
		(ComponentMapper<T>)GetMapperNoTypeCheck(typeof(T));

	/// <summary>
	///     Get the component mapper for the given singleton component.
	/// </summary>
	/// <typeparam name="T">The type of component</typeparam>
	/// <returns>the component mapper for the component type T</returns>
	public SingletonComponentMapper<T> GetSingletonMapper<T>() where T : ISingletonComponent, new() =>
		(SingletonComponentMapper<T>)GetMapperNoTypeCheck(typeof(T));

	public IComponentMapper GetMapper(Type componentType)
	{
		if (!componentType.IsAssignableTo(typeof(IComponent)))
		{
			throw new ArgumentException(
				$"Argument {nameof(componentType)} of type {componentType} does not match type constraint '{nameof(IComponent)}'");
		}

		return GetMapperNoTypeCheck(componentType);
	}

	public ref T GetSingletonComponent<T>() where T : ISingletonComponent, new() =>
		ref GetSingletonMapper<T>().SingletonInstance;

	public ISingletonComponent GetSingletonComponent(Type componentType)
	{
		if (!componentType.IsAssignableTo(typeof(ISingletonComponent)) ||
		    GetMapper(componentType).Get(Entity.SingletonEntity) is not ISingletonComponent singletonComponent)
		{
			throw new ArgumentException(
				$"Argument {nameof(componentType)} of type {componentType} does not match type constraint '{nameof(ISingletonComponent)}'");
		}

		return singletonComponent;
	}

	private IComponentMapper GetMapperNoTypeCheck(Type componentType)
	{
		if (mappers.TryGetValue(componentType, out var mapper))
		{
			return mapper;
		}

		var mapperType = componentType.IsAssignableTo(typeof(ISingletonComponent))
			? typeof(SingletonComponentMapper<>).MakeGenericType(componentType)
			: typeof(ComponentMapper<>).MakeGenericType(componentType);
		var newMapper = (IComponentMapper)Activator.CreateInstance(mapperType, this)!;
		mappers.Add(componentType, newMapper);
		return newMapper;
	}

	/// <summary>
	///     Get the component flags for the given entity.
	/// </summary>
	/// <param name="entity">the entity</param>
	/// <returns>the component flags for the given entity</returns>
	public ComponentFlags GetFlagsForEntity(Entity entity)
	{
		if (EntityIsDirty(entity))
		{
			HydrateEntity(entity);
		}

		return entityFlags[entity];
	}

	/// <summary>
	///     Get the entities in this <c>World</c> that belong to the given aspect.
	/// </summary>
	/// <param name="aspect">the aspect</param>
	/// <returns>the entities</returns>
	public SnapshotList<Entity> GetEntitiesForAspect(Aspect aspect)
	{
		UpdateDirtyEntities();

		if (entitiesForAspects.TryGetValue(aspect, out var list))
		{
			return list;
		}

		list = new SnapshotList<Entity>();
		entitiesForAspects.Add(aspect, list);

		foreach (var entity in entities)
		{
			if (aspect.Matches(this, entity))
			{
				list.Add(entity);
			}
		}

		return list;
	}

	private ComponentFlags ConstructFlagsForEntity(Entity entity)
	{
		var builder = new ComponentFlagsBuilder();
		foreach (var (_, mapper) in mappers)
		{
			builder.Set(mapper.ComponentId, mapper.Has(entity));
		}

		return builder;
	}

	/// <summary>
	///     Marks the given entity as "dirty"; i.e. needing re-calculation. You probably shouldn't need to use this manually,
	///     this is handled internally.
	/// </summary>
	/// <param name="entity">the entity</param>
	private void MarkEntityDirty(Entity entity) => dirtyEntities.Add(entity);

	/// <summary>
	///     Registers that this entity just has a component added to it, and will require re-calculation.
	/// </summary>
	/// <param name="entity">the entity</param>
	public void ComponentAdded(Entity entity) => dirtyEntities.Add(entity);

	/// <summary>
	///     Registers that this entity just has a component removed from it, and will require re-calculation.
	/// </summary>
	/// <param name="entity">the entity</param>
	public void ComponentRemoved(Entity entity) => dirtyEntities.Add(entity);

	/// <summary>
	///     "Hydrates" the given entity, i.e. updating flags & aspects etc. You probably shouldn't need to use this manually,
	///     this is handled internally.
	/// </summary>
	/// <param name="entity">the entity</param>
	public void HydrateEntity(Entity entity)
	{
		// Need to remove this up here so we don't get an infinite cycle.
		dirtyEntities.Remove(entity);

		// Cache flags
		var flags = ConstructFlagsForEntity(entity);
		if (!entityFlags.ContainsKey(entity))
		{
			entityFlags.Add(entity, flags);
		}
		else
		{
			entityFlags[entity] = flags;
		}

		// Update aspects
		foreach (var (aspect, aspectEntities) in entitiesForAspects)
		{
			var matches = aspect.Matches(this, entity);
			var contains = aspectEntities.Contains(entity);
			if (matches == contains)
			{
				continue;
			}

			if (matches)
			{
				aspectEntities.Add(entity);
			}
			else
			{
				aspectEntities.Remove(entity);
			}
		}
	}

	/// <summary>
	///     Checks if the given entity is dirty and in need of hydration. You probably shouldn't need to check this
	///     manually, this is handled internally.
	/// </summary>
	/// <param name="entity">the entity</param>
	/// <returns>
	///     <c>true</c> if the entity is dirty, <c>false</c> otherwise
	/// </returns>
	public bool EntityIsDirty(Entity entity) => dirtyEntities.Contains(entity);
}