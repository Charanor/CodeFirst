namespace CodeFirst.Ecs.Core;

/// <summary>
///     Represents an entity in the ECS. This is a very light-weight wrapper.
/// </summary>
/// <remarks>
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
	public static readonly Entity SingletonEntity = default;

	internal Entity(int id)
	{
		Id = id;
		IsValid = Id >= 0;
	}

	/// <summary>
	///     The internal Id of the entity. It is guaranteed to be unique and non-negative for all <b>valid</b> entities.
	/// </summary>
	/// <remarks>
	///     Makes no guarantees of what the value will be for invalid entities.
	/// </remarks>
	internal int Id { get; }

	/// <summary>
	///     If the entity is a valid entity or not.
	/// </summary>
	/// <remarks>
	///     Note that while some interfaces within the ECS will happily allow you to pass an invalid entity, it makes no
	///     guarantees that the result will be consistent or well-behaved, so any interfacing with the ECS using an
	///     invalid entity should be treated as undefined behavior.
	/// </remarks>
	public bool IsValid { get; }

	public bool Equals(Entity other) => Id == other.Id && IsValid == other.IsValid;

	public override int GetHashCode() => Id;

	public override string ToString() => this == SingletonEntity
		? "SingletonEntity"
		: $"{(IsValid ? string.Empty : "Invalid")}Entity({Id})";
}