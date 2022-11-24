using System.Diagnostics;
using System.Reflection;
using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Utilities;
using JXS.Utils.Collections;
using JXS.Utils.Events;

namespace JXS.Ecs.Core;

/// <summary>
///     The base entity system class. The entity system is responsible for processing groups of entities defined
///     by an <see cref="Aspect" />.
/// </summary>
public abstract class EntitySystem
{
	private SnapshotList<int> entities;

	protected EntitySystem(Aspect aspect, Pass pass)
	{
		Pass = pass;
		Aspect = aspect;
		// This looks weird, but we need to set "entities" first so we don't get a null reference exception
		Entities = entities = new SnapshotList<int>();
	}

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
	protected EntitySystem()
	{
		Pass = GetPassFromAttribute();
		Aspect = GetAspectFromAttributes();
		// This looks weird, but we need to set "entities" first so we don't get a null reference exception
		Entities = entities = new SnapshotList<int>();
	}

	/// <summary>
	///     During which pass this system should update.
	/// </summary>
	public Pass Pass { get; }

	/// <summary>
	///     The aspect of entities this system processes.
	/// </summary>
	public Aspect Aspect { get; }

	/// <summary>
	///     The entities that this system will process.
	///     Guaranteed to be the same reference, **unless the system is added to another world (or after it's added the
	///     first time)**. This list is the real thing so don't modify it directly unless you know what you're doing.
	/// </summary>
	/// <remarks>
	///     Modifying this value will call <see cref="EntityAdded" /> and/or <see cref="EntityRemoved" /> for each new
	///     entity added or removed, respectively. Will not be called for entities present in both the old and new list.
	/// </remarks>
	public SnapshotList<int> Entities
	{
		get => entities;
		internal set
		{
			var prevEntities = entities;
			var removedEntities = prevEntities.Where(val => !value.Contains(val));
			foreach (var removedEntity in removedEntities)
			{
				EntityRemoved(removedEntity);
			}

			entities = value;
			var addedEntities = value.Where(val => !prevEntities.Contains(val));
			foreach (var addedEntity in addedEntities)
			{
				EntityAdded(addedEntity);
			}

			if (prevEntities != entities)
			{
				// New reference, re-target event listeners
				prevEntities.OnItemAdded -= OnItemAdded;
				prevEntities.OnItemRemoved -= OnItemRemoved;

				value.OnItemAdded += OnItemAdded;
				value.OnItemRemoved += OnItemRemoved;
			}
		}
	}

	/// <summary>
	///     The world that this system belongs to. Null if it does not belong to a world.
	/// </summary>
	public World? World { get; internal set; }

	/// <summary>
	///     If <code>false</code> this system will not be updated.
	/// </summary>
	public bool Enabled { get; set; } = true;

	public abstract void Update(float delta);

	private Pass GetPassFromAttribute()
	{
		var type = GetType();
		var pass = type.GetCustomAttribute<ProcessPassAttribute>()?.Pass;
		if (pass == null)
		{
			throw new ArgumentNullException(
				$"{nameof(EntitySystem)}'s pass must not be null. Use [{nameof(ProcessPassAttribute)}({nameof(Pass)})] Attribute or pass a {nameof(Pass)} to the constructor.");
		}

		return pass.Value;
	}

	private Aspect GetAspectFromAttributes()
	{
		var type = GetType();
		var all = type.GetCustomAttribute<AllAttribute>();
		var one = type.GetCustomAttribute<SomeAttribute>();
		var none = type.GetCustomAttribute<NoneAttribute>();

		var builder = new AspectBuilder();
		builder.All(all?.Types ?? Array.Empty<Type>());
		builder.Some(one?.Types ?? Array.Empty<Type>());
		builder.None(none?.Types ?? Array.Empty<Type>());
		return builder.Build();
	}

	/// <summary>
	///     Called when the system begins processing, right before <c>EntitySystem#Update</c>.
	/// </summary>
	public virtual void Begin()
	{
	}

	/// <summary>
	///     Called when the systems end processing, right after <c>EntitySystem#Update</c>.
	/// </summary>
	public virtual void End()
	{
	}

	/// <summary>
	///     Called every time the system is added to a World. The <see cref="Entities" /> and <see cref="World" />
	///     values are guaranteed to be initialized when this is called, as well as any injected dependencies.
	/// </summary>
	/// <param name="world">the world this system was added to. Functionally identical to <see cref="World" /></param>
	public virtual void Initialize(World world)
	{
	}

	/// <summary>
	///     Called when an entity that will be processed by this EntitySystem is added to the parent <see cref="World" />.
	/// </summary>
	/// <remarks>
	///     This <i>could</i> be called before <see cref="Initialize" /> is called (e.g. when first adding this EntitySystem to
	///     the
	///     World). However it is guaranteed that:
	///     <list type="bullet">
	///         <item>
	///             <description><see cref="World" /> is non-null.</description>
	///         </item>
	///         <item>
	///             <description><see cref="Entities" /> is non-null (but should not be modified).</description>
	///         </item>
	///         <item>
	///             <description>Any dependencies are injected.</description>
	///         </item>
	///     </list>
	/// </remarks>
	/// <param name="entity">the entity that was added</param>
	protected virtual void EntityAdded(int entity)
	{
	}

	/// <summary>
	///     Called when an entity that was processed by this EntitySystem is removed from the parent <see cref="World" />.
	/// </summary>
	/// <remarks>
	///     Unlike <see cref="EntityAdded" />, this function will not be called until after <see cref="Initialize" />
	///     has been called.
	/// </remarks>
	/// <param name="entity">the entity that was added</param>
	protected virtual void EntityRemoved(int entity)
	{
	}

	protected virtual void Remove(int entity)
	{
		Debug.Assert(World != null);
		World?.DeleteEntity(entity);
	}

	private void OnItemRemoved(SnapshotList<int> _, EventArgs<int> e) => EntityRemoved(e.Value);
	private void OnItemAdded(SnapshotList<int> _, EventArgs<int> e) => EntityAdded(e.Value);
}