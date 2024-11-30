using System.Diagnostics;
using System.Reflection;
using CodeFirst.Ecs.Core.Attributes;

namespace CodeFirst.Ecs.Core;

/// <summary>
///     The base entity system class.
/// </summary>
/// <remarks>
///     If a system implements <see cref="IDisposable" /> it will be disposed of when
///     <see cref="CodeFirst.Ecs.Core.World.Dispose">World.Dispose</see> is called.
/// </remarks>
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
	///     If <c>false</c> this system will not be updated.
	/// </summary>
	/// <remarks>
	///     Be aware that this property is only respected if <see cref="ShouldUpdate" /> uses it, so if you are
	///     overriding <see cref="ShouldUpdate" /> make sure to take <see cref="Enabled" /> into account.
	/// </remarks>
	/// <seealso cref="ShouldUpdate" />
	public bool Enabled { get; set; } = true;

	public abstract void Update(float delta);

	/// <summary>
	///     Checks if this system should update or not. The default implementation simply checks if the entity system is
	///     enabled.
	/// </summary>
	/// <remarks>
	///     Make sure to take <see cref="Enabled" /> into account in any overwritten implementation.
	/// </remarks>
	/// <returns><c>true</c> if the system should update, <c>false</c> otherwise</returns>
	/// <seealso cref="Enabled" />
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
	///     Called when the system begins processing, right before <see cref="EntitySystem.Update" />.
	/// </summary>
	/// <seealso cref="Update" />
	public virtual void Begin()
	{
	}

	/// <summary>
	///     Called when the system stops processing, right after <see cref="EntitySystem.Update" />.
	/// </summary>
	/// <seealso cref="Update" />
	public virtual void End()
	{
	}

	/// <summary>
	///     Called every time the system is added to a World. The <see cref="World" /> property is guaranteed to be
	///     initialized when this is called, as well as any injected dependencies.
	/// </summary>
	/// <param name="world">the world this system was added to. Functionally identical to <see cref="World" /></param>
	public virtual void Initialize(World world)
	{
	}

	/// <summary>
	///     Utility method to remove the given entity from the world.
	/// </summary>
	/// <param name="entity">the entity to remove</param>
	protected virtual void Remove(Entity entity)
	{
		Debug.Assert(World != null);
		World?.DeleteEntity(entity);
	}
}