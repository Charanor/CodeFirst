using System.Diagnostics;
using System.Reflection;
using CodeFirst.Ecs.Core.Attributes;

namespace CodeFirst.Ecs.Core;

/// <summary>
///     The base entity system class. The entity system is responsible for processing groups of entities defined
///     by an <see cref="Aspect" />.
/// </summary>
public abstract class EntitySystem
{
	protected EntitySystem(Pass pass)
	{
		Pass = pass;
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
	}

	/// <summary>
	///     During which pass this system should update.
	/// </summary>
	public Pass Pass { get; }

	/// <summary>
	///     The world that this system belongs to. Null if it does not belong to a world.
	/// </summary>
	public World? World { get; internal set; }

	/// <summary>
	///     If <code>false</code> this system will not be updated.
	/// </summary>
	public bool Enabled { get; set; } = true;

	public abstract void Update(float delta);

	/// <summary>
	///     Checks if this system should update or not. The default implementation simply checks if the entity system is
	///     enabled.
	/// </summary>
	/// <returns><c>true</c> if the system should update, <c>false</c> otherwise</returns>
	public virtual bool ShouldUpdate() => Enabled;

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
	protected virtual void EntityAdded(Entity entity)
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
	protected virtual void EntityRemoved(Entity entity)
	{
	}

	protected virtual void Remove(Entity entity)
	{
		Debug.Assert(World != null);
		World?.DeleteEntity(entity);
	}
}