using System.Reflection;
using JXS.Ecs.Core.Exceptions;
using JXS.Ecs.Core.Utilities;
using JXS.Utils;
using JXS.Utils.Collections;

namespace JXS.Ecs.Core;

/// <summary>
///     The core class of the ECS. Handles updating all systems and managing the components of entities.
/// </summary>
public class World : IDisposable
{
	private readonly ISet<Entity> entities;
	private readonly ISet<Entity> removedEntities;
	private readonly ISet<Entity> dirtyEntities;

	private readonly IDictionary<Type, IComponentMapper> mappers;
	private readonly IDictionary<Entity, ComponentFlags> entityFlags;

	private readonly IDictionary<Aspect, SnapshotList<Entity>> entitiesForAspects;

	private float fixedUpdateAccumulator;

	public World()
	{
		entities = new HashSet<Entity>();
		removedEntities = new HashSet<Entity>();

		UpdateSystems = new List<EntitySystem>();
		FixedUpdateSystems = new List<EntitySystem>();
		DrawSystems = new List<EntitySystem>();
		dirtyEntities = new HashSet<Entity>();

		mappers = new Dictionary<Type, IComponentMapper>();
		entityFlags = new Dictionary<Entity, ComponentFlags>();

		entitiesForAspects = new Dictionary<Aspect, SnapshotList<Entity>>();
	}

	protected IList<EntitySystem> UpdateSystems { get; }
	protected IList<EntitySystem> FixedUpdateSystems { get; }
	protected IList<EntitySystem> DrawSystems { get; }

	/// <summary>
	///     The delta time of the fixed update step. For example a value of <c>0.01</c> would mean that the fixed update
	///     runs 100 times per second (1 / 0.01). If <c>&lt;=0</c> fixed update will not run at all.
	/// </summary>
	/// <remarks>
	///     The fixed update is controlled by the <see cref="Update" /> function and run directly after the normal update
	///     pass (if it is its time to run, that is).
	/// </remarks>
	public float FixedUpdateDelta { get; set; }

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		// ReSharper disable once SuspiciousTypeConversion.Global
		foreach (var system in DrawSystems.OfType<IDisposable>())
		{
			system.Dispose();
		}

		// ReSharper disable once SuspiciousTypeConversion.Global
		foreach (var system in UpdateSystems.OfType<IDisposable>())
		{
			system.Dispose();
		}

		// ReSharper disable once SuspiciousTypeConversion.Global
		foreach (var system in FixedUpdateSystems.OfType<IDisposable>())
		{
			system.Dispose();
		}
	}

	/// <summary>
	///     Draws all Systems with pass <c>Pass.Update</c>.
	/// </summary>
	/// <param name="delta">time since last Update()</param>
	public void Update(float delta)
	{
		foreach (var system in UpdateSystems)
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

		if (FixedUpdateDelta > 0)
		{
			fixedUpdateAccumulator += delta;
			// We cache the delta here in case it is changed during processing, which could have weird effects
			var currentFixedUpdateDelta = FixedUpdateDelta;
			for (; fixedUpdateAccumulator >= currentFixedUpdateDelta; fixedUpdateAccumulator -= currentFixedUpdateDelta)
			{
				UpdateFixed(currentFixedUpdateDelta);
			}
		}
		else
		{
			fixedUpdateAccumulator = 0;
		}

		RemoveDeletedEntities();
	}

	private void UpdateFixed(float delta)
	{
		foreach (var system in FixedUpdateSystems)
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
	}

	/// <summary>
	///     Draws all Systems with pass <c>Pass.Draw</c>.
	/// </summary>
	/// <param name="delta">time since last Draw()</param>
	public void Draw(float delta)
	{
		foreach (var system in DrawSystems)
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
			entityFlags.Remove(removedEntity);
			dirtyEntities.Remove(removedEntity);

			foreach (var (_, aspectEntities) in entitiesForAspects)
			{
				if (aspectEntities.Contains(removedEntity))
				{
					aspectEntities.Remove(removedEntity);
				}
			}
		}

		removedEntities.Clear();
	}

	public bool HasEntity(Entity entity) => entity.IsValid && entities.Contains(entity);

	/// <summary>
	///     Creates a new entity and adds it to the world.
	/// </summary>
	/// <param name="id">
	///     the optional id of the entity. If left as default, or <c>&lt;0</c>, will pick the 1st available id. This
	///     parameter is, for the most part, unnecessary to set and is primarily offered as a way to re-initialize a
	///     world from some state, or to synchronize entity ID:s across server/client pairs.
	/// </param>
	/// <returns>the created entity. In non-dev environments this may be <see cref="Entity.Invalid" /> if an error occurs.</returns>
	/// <exception cref="IndexOutOfRangeException">
	///     (Dev only) If we have ran out of new entity ID:s (
	///     <c>int.MaxValue = 2147483647</c>)
	/// </exception>
	/// <exception cref="EntityAlreadyExistsException">(Dev only) An entity with the given ID already exists</exception>
	public Entity CreateEntity(int id = -1)
	{
		if (id < 0)
		{
			// Search for 1st valid ID
			for (var i = 0; i < int.MaxValue; i++)
			{
				if (entities.Contains(new Entity(i)))
				{
					continue;
				}

				id = i;
				break;
			}

			if (id < 0)
			{
				DevTools.Throw<World>(new IndexOutOfRangeException($"Max number of entities reached, {int.MaxValue}"));
				return Entity.Invalid;
			}
		}

		var entity = new Entity(id);
		if (entities.Contains(entity))
		{
			DevTools.Throw<World>(new EntityAlreadyExistsException(entity));
			return Entity.Invalid;
		}

		entities.Add(entity);
		MarkEntityDirty(entity);
		return entity;
	}

	/// <summary>
	///     Marks this entity to be deleted from this <c>World</c> at the end of the current processing stage (update or
	///     draw).
	/// </summary>
	/// <param name="entity">the entity</param>
	/// <exception cref="InvalidEntityException">(Dev only) if <see cref="Entity.IsValid" /> is false</exception>
	public void DeleteEntity(Entity entity)
	{
		if (!entity.IsValid)
		{
			DevTools.Throw<World>(new InvalidEntityException());
			return;
		}

		removedEntities.Add(entity);
	}

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
				if (UpdateSystems.Any(sys => sys.GetType() == system.GetType()))
				{
					throw new InvalidOperationException(
						$"A system of type {system.GetType()} already exists in world.");
				}

				UpdateSystems.Add(system);
				break;
			case Pass.FixedUpdate:
				if (FixedUpdateSystems.Any(sys => sys.GetType() == system.GetType()))
				{
					throw new InvalidOperationException(
						$"A system of type {system.GetType()} already exists in world.");
				}

				FixedUpdateSystems.Add(system);
				break;
			case Pass.Draw:
				if (DrawSystems.Any(sys => sys.GetType() == system.GetType()))
				{
					throw new InvalidOperationException(
						$"A system of type {system.GetType()} already exists in world.");
				}

				DrawSystems.Add(system);
				break;
			default:
				throw new InvalidOperationException($"Invalid {nameof(system.Pass)} {system.Pass}");
		}

		Inject(system);
		system.World = this;
		system.Initialize(this);
	}

	public bool HasSystem<TSystem>() where TSystem : EntitySystem => HasSystem(typeof(TSystem));

	public bool HasSystem(Type type) =>
		UpdateSystems.Any(updateSystem => updateSystem.GetType() == type) ||
		FixedUpdateSystems.Any(updateSystem => updateSystem.GetType() == type) ||
		DrawSystems.Any(updateSystem => updateSystem.GetType() == type);

	/// <summary>
	///     Injects any injectable dependencies registered with this world into the given object,
	///     for example component mappers. Fields that receive injected dependencies must be marked as 'readonly'.
	///     <code>
	///  		// If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///   	private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	///    </code>
	/// </summary>
	/// <param name="obj">The object to inject the dependencies into</param>
	/// <exception cref="InjectionException">If the field definition of the injected dependency is not 'readonly'</exception>
	/// <example>
	///     // If you are using nullable context, initialise the value to "null!" to prevent nullability warnings :)
	///     <br />
	///     private readonly ComponentMapper&lt;MyComponent&gt; myComponentMapper = null!;
	/// </example>
	/// <seealso cref="InjectComponentMappers" />
	public void Inject(object obj)
	{
		InjectComponentMappers(obj);
		InjectAspectsAndEntities(obj);
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
	/// <exception cref="InjectionException">(Dev only) If the component mapper field definition is not 'readonly'</exception>
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
				DevTools.Throw<World>(new InjectionException(fieldInfo,
					$"Could not inject {obj} into field {fieldInfo}: Field is not 'readonly'!"));
				continue;
			}

			fieldInfo.SetValue(obj, GetMapper(mapperType.GenericTypeArguments[0]));
		}
	}

	/// <exception cref="InjectionException">
	///     (Dev only) If the injected field definition is not 'readonly', or if an
	///     aspect-annotated field is not of type <see cref="Aspect" /> or <see cref="IReadOnlySnapshotList{T}" />
	/// </exception>
	public void InjectAspectsAndEntities(object obj)
	{
		var type = obj.GetType();
		foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
		{
			var fieldType = fieldInfo.FieldType;
			var aspect = AspectBuilder.GetAspectFromFieldAttributes(fieldInfo);
			if (aspect.IsEmpty)
			{
				// Not annotated, can't inject
				continue;
			}

			if (!fieldInfo.IsInitOnly)
			{
				DevTools.Throw<World>(new InjectionException(fieldInfo,
					$"Could not inject {obj} into field {fieldInfo}: Field is not 'readonly'!"));
				continue;
			}

			if (fieldType.IsAssignableTo(typeof(IReadOnlySnapshotList<Entity>)))
			{
				fieldInfo.SetValue(obj, GetEntitiesForAspect(aspect));
			}
			else if (fieldType.IsAssignableTo(typeof(Aspect)))
			{
				fieldInfo.SetValue(obj, aspect);
			}
			else
			{
				// We can't inject an aspect into a field of this type	
				DevTools.Throw<World>(new InjectionException(fieldInfo,
					$"Can not inject aspect-annotated field of type {fieldType}. Expected field type to be one of [{nameof(Aspect)}, {nameof(IReadOnlySnapshotList<Entity>)}]"));
			}
		}
	}

	public void RemoveSystem<TSystem>() where TSystem : EntitySystem => RemoveSystem(typeof(TSystem));

	/// <summary>
	///     Removes a system from this world.
	/// </summary>
	/// <remarks>If the system doesn't exist, nothing happens.</remarks>
	/// <param name="type">the type of system to remove</param>
	public void RemoveSystem(Type type)
	{
		var system = UpdateSystems.FirstOrDefault(updateSystem => updateSystem.GetType() == type);
		if (system != null)
		{
			UpdateSystems.Remove(system);
			return;
		}

		system = FixedUpdateSystems.FirstOrDefault(fixedUpdateSystem => fixedUpdateSystem.GetType() == type);
		if (system != null)
		{
			FixedUpdateSystems.Remove(system);
		}

		system = DrawSystems.FirstOrDefault(drawSystem => drawSystem.GetType() == type);
		if (system != null)
		{
			DrawSystems.Remove(system);
		}
	}

	/// <summary>
	///     Removes a system from this world.
	/// </summary>
	/// <remarks>If the system doesn't exist, nothing happens.</remarks>
	/// <param name="system">the system to remove</param>
	public void RemoveSystem(EntitySystem system)
	{
		UpdateSystems.Remove(system);
		FixedUpdateSystems.Remove(system);
		DrawSystems.Remove(system);
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
	/// <exception cref="InvalidEntityException">(Dev only) if <see cref="Entity.IsValid" /> is <c>false</c></exception>
	public ComponentFlags GetFlagsForEntity(Entity entity)
	{
		if (!entity.IsValid)
		{
			DevTools.Throw<World>(new InvalidEntityException());
			return ComponentFlags.Empty;
		}

		if (!HasEntity(entity))
		{
			return ComponentFlags.Empty;
		}

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
	public IReadOnlySnapshotList<Entity> GetEntitiesForAspect(Aspect aspect) => GetMutableEntitiesForAspect(aspect);

	private ISnapshotList<Entity> GetMutableEntitiesForAspect(Aspect aspect)
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
	private void MarkEntityDirty(Entity entity)
	{
		if (!HasEntity(entity))
		{
			DevTools.Throw<World>(new EntityDoesNotExistException(entity));
			return;
		}

		dirtyEntities.Add(entity);
	}

	/// <summary>
	///     Registers that this entity just has a component added to it, and will require re-calculation.
	/// </summary>
	/// <param name="entity">the entity</param>
	internal void ComponentAdded(Entity entity) => MarkEntityDirty(entity);

	/// <summary>
	///     Registers that this entity just has a component removed from it, and will require re-calculation.
	/// </summary>
	/// <param name="entity">the entity</param>
	internal void ComponentRemoved(Entity entity) => MarkEntityDirty(entity);

	/// <summary>
	///     "Hydrates" the given entity, i.e. updating flags & aspects etc. You probably shouldn't need to use this manually,
	///     this is handled internally.
	/// </summary>
	/// <param name="entity">the entity</param>
	/// <exception cref="InvalidEntityException">(Dev only) if <see cref="Entity.IsValid" /> is <c>false</c></exception>
	/// <exception cref="EntityDoesNotExistException">(Dev only) if entity does not exist in this world</exception>
	public void HydrateEntity(Entity entity)
	{
		if (!entity.IsValid)
		{
			DevTools.Throw<World>(new InvalidEntityException());
			return;
		}

		if (!HasEntity(entity))
		{
			DevTools.Throw<World>(new EntityDoesNotExistException(entity));
			return;
		}

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
	public bool EntityIsDirty(Entity entity) => entity.IsValid && dirtyEntities.Contains(entity);

	private static Exception CreateEntityDoesNotExistException(Entity entity) =>
		new ArgumentException($"Entity {entity} does not exist in world!", nameof(entity));
}