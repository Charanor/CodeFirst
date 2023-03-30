using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Utilities;
using JXS.Utils.Collections;
using JXS.Utils.Events;

namespace JXS.Ecs.Core;

/// <summary>
///     An <see cref="EntitySystem" /> that iterates over the entities in its <see cref="Aspect" /> one-by-one.
/// </summary>
public abstract class IteratingSystem : EntitySystem
{
	private IReadOnlySnapshotList<Entity> internalEntities;
	
	/// <summary>
	///     Constructs a new EntitySystem and infers Family and Pass from the <c>[ProcessPass(Pass)]</c>,
	///     <c>[All(ComponentType[])]</c>,
	///     <c>[One(ComponentType[])]</c>, and <c>[None(ComponentType[])]</c> attributes.
	/// </summary>
	/// <remarks><c>[ProcessPass(Pass)]</c> attribute is mandatory.</remarks>
	/// <seealso cref="ProcessPassAttribute" />
	/// <seealso cref="AllAttribute" />
	/// <seealso cref="SomeAttribute" />
	/// <seealso cref="NoneAttribute" />
	protected IteratingSystem()
	{
		Aspect = AspectBuilder.GetAspectFromAttributes(GetType());
		// This looks weird, but we need to set "entities" first so we don't get a null reference exception
		Entities = internalEntities = new SnapshotList<Entity>();
	}

	protected IteratingSystem(Aspect aspect, Pass pass) : base(pass)
	{
		Aspect = aspect;
		// This looks weird, but we need to set "entities" first so we don't get a null reference exception
		Entities = internalEntities = new SnapshotList<Entity>();
	}

	/// <summary>
	///     The aspect of entities this system processes.
	/// </summary>
	public Aspect Aspect { get; }

	/// <summary>
	///     The entities that this system will process.
	///     Guaranteed to be the same reference, **unless the system is added to another world (or after it's added the
	///     first time)**.
	/// </summary>
	public IReadOnlySnapshotList<Entity> Entities
	{
		get => internalEntities;
		internal set
		{
			var prevEntities = internalEntities;
			var removedEntities = prevEntities.Where(val => !value.Contains(val));
			foreach (var removedEntity in removedEntities)
			{
				EntityRemoved(removedEntity);
			}

			internalEntities = value;
			var addedEntities = value.Where(val => !prevEntities.Contains(val));
			foreach (var addedEntity in addedEntities)
			{
				EntityAdded(addedEntity);
			}

			if (prevEntities != internalEntities)
			{
				// New reference, re-target event listeners
				prevEntities.OnItemAdded -= OnItemAdded;
				prevEntities.OnItemRemoved -= OnItemRemoved;

				value.OnItemAdded += OnItemAdded;
				value.OnItemRemoved += OnItemRemoved;
			}
		}
	}

	protected Entity CurrentEntity { get; set; }
	
	public override void Initialize(World world)
	{
		base.Initialize(world);
		Entities = world.GetEntitiesForAspect(Aspect);
	}

	protected abstract void Update(Entity entity, float delta);

	public override void Update(float delta)
	{
		var (entities, size) = Entities.Begin();

		for (var i = 0; i < size; i++)
		{
			var entity = entities[i];
			CurrentEntity = entity;
			Update(entity, delta);
			CurrentEntity = Entity.Invalid;
		}

		Entities.Commit();
	}

	protected void AssertHasEntity(string methodName)
	{
		if (!CurrentEntity.IsValid)
		{
			throw new InvalidOperationException($"Can not call {methodName} while not processing an entity!");
		}
	}

	protected sealed override void Remove(Entity entity)
	{
		AssertHasEntity(nameof(Remove));
		base.Remove(entity);
	}

	protected void RemoveEntity()
	{
		AssertHasEntity(nameof(RemoveEntity));
		base.Remove(CurrentEntity);
	}

	public bool IsCurrentEntity(Entity entity) => entity == CurrentEntity;

	private void OnItemRemoved(SnapshotList<Entity> _, EventArgs<Entity> e) => EntityRemoved(e.Value);
	private void OnItemAdded(SnapshotList<Entity> _, EventArgs<Entity> e) => EntityAdded(e.Value);
}