using CodeFirst.Utils;

namespace CodeFirst.Ecs.Core;

/// <summary>
///     Represents an entity in the ECS. This is a very light-weight wrapper around an unsigned integer.
/// </summary>
/// <remarks>
///     <c>Entity</c> is opaque and should not be used as a stable reference to an Entity, for example it should not be
///     saved in a collection or in a field within an object. This is because the <see cref="World" /> gives no guarantee
///     that an Entity will not be re-used in a subsequent frame for a different Entity (this can happen if ID:s are
///     defragmented, for example). If you need a stable reference to an Entity, e.g. if a projectile needs to store a
///     reference of which Entity to follow, create a custom component that stores a unique ID of some kind (e.g. a GUID)
///     and then check against that: <c>GetComponent&lt;Id&gt;(entity1).Id == GetComponent&lt;Id&gt;(entity2).Id</c>.
///     <br /><br />
///     Note that <c>default</c> is an <b>invalid</b> entity, so any uninitialised values of Entity are invalid.
/// </remarks>
public readonly record struct Entity
{
	/// <summary>
	///     Represents an <b>invalid</b> entity. Useful for scenarios where you need a "null" or "empty" entity.
	/// </summary>
	public static readonly Entity Invalid = new(-1);

	/// <summary>
	///     Represents the entity used when interfacing with singleton component mappers.
	///     This is an <b>invalid</b> entity.
	/// </summary>
	/// <remarks>
	///     This is the ONLY <b>invalid</b> entity that has well-defined behavior, and that behavior is valid
	///     <b>when and ONLY when</b> interfacing with <see cref="SingletonComponentMapper{T}" /> instances!
	/// </remarks>
	public static readonly Entity SingletonEntity = new(-2);

	internal Entity(int id)
	{
		Id = id;
		IsValid = Id >= 0;
	}

	/// <summary>
	///     The internal Id of the entity. It is guaranteed to be unique (but not stable! See struct documentation
	///     <see cref="Entity" />)
	///     and non-negative for all <b>valid</b> entities.
	/// </summary>
	/// <remarks>
	///     Makes no guarantees of what the value will be for invalid entities.
	/// </remarks>
	internal int Id { get; }

	/// <summary>
	///     If the entity is a valid entity or not. Be very aware that just because an Entity is valid that does NOT
	///     mean that the entity exists within the world, it simply means that the Entity was created in a valid way
	///     (i.e. from a <see cref="World" /> instead of <c>default</c> or something).
	/// </summary>
	/// <remarks>
	///     While some interfaces within the ECS will happily allow you to pass an invalid entity, it makes no
	///     guarantees that the result will be consistent or well-behaved, so any interfacing with the ECS using an
	///     invalid entity should be treated as undefined behavior. Most methods will throw an exception in dev mode
	///     (see <see cref="DevTools.IsDevMode">DevTools.IsDevMode</see>) but will silently fail in non-dev mode.
	/// </remarks>
	public bool IsValid { get; }

	public bool Equals(Entity other) => Id == other.Id && IsValid == other.IsValid;

	public override int GetHashCode() => Id;

	public override string ToString() => this == SingletonEntity
		? "SingletonEntity"
		: $"{(IsValid ? string.Empty : "Invalid")}Entity({Id})";
}