using JXS.Ecs.Core.Exceptions;

namespace JXS.Ecs.Core;

public interface IComponentMapper
{
	public int ComponentId { get; }

	/// <summary>
	///     Gets a component from this mapper
	/// </summary>
	/// <param name="entity">the entity id</param>
	/// <returns>the component that belongs to the specified entity</returns>
	/// <exception cref="NullReferenceException">thrown if the entity does not contain this component type.</exception>
	public IComponent Get(Entity entity);

	/// <summary>
	///     Updates the component that belong to this entity with any changes from <c>component</c>. Can also be used to add a
	///     pre-constructed component instead of relying on <c>Create</c>.
	/// </summary>
	/// <param name="entity">the entity id</param>
	/// <param name="component">the component to update values for</param>
	public void Update(Entity entity, in IComponent component);

	/// <summary>
	///     Creates a new component of the given type and returns it. Will override existing component.
	/// </summary>
	/// <param name="entity">the entity id</param>
	/// <returns>the newly created component</returns>
	/// <exception cref="NotDefaultConstructibleException{T}">
	///     If the component this mapper is bound to can not be 0-argument
	///     constructed and is not a value type.
	/// </exception>
	public IComponent Create(Entity entity);

	/// <summary>
	///     Adds the given component to the entity. Useful when you need to add a ready-created component to the entity.
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="component"></param>
	/// <returns></returns>
	public IComponent Add(Entity entity, in IComponent component);

	/// <summary>
	///     If the given entity does not have this component, will add the given component to the entity and return it.
	///     Otherwise returns the existing instance of the component.
	/// </summary>
	/// <param name="entity">the entity</param>
	/// <param name="component">the component to add</param>
	/// <returns>the component</returns>
	public IComponent AddIfMissing(Entity entity, in IComponent component);

	/// <summary>
	///     Removes the component from the given entity. Does nothing if the entity did not have the component already.
	/// </summary>
	/// <param name="entity">the entity id</param>
	public void Remove(Entity entity);

	/// <summary>
	///     Checks if the entity has a component.
	/// </summary>
	/// <param name="entity">the entity id</param>
	/// <returns><c>true</c> if the entity has this component, <c>false</c> otherwise</returns>
	public bool Has(Entity entity);

	/// <summary>
	///     Sets or removes ownership of this component for the given entity.
	/// </summary>
	/// <remarks>If <c>shouldHave</c> is <c>true</c>, will create a NEW instance of the component</remarks>
	/// <param name="entity">the entity id</param>
	/// <param name="shouldHave">
	///     if <c>true</c>, ensures that this entity has this component. If <c>false</c> ensures that this
	///     entity does not have this component.
	/// </param>
	/// <exception cref="NotDefaultConstructibleException{T}">
	///     If the component this mapper is bound to can not be 0-argument
	///     constructed and is not a value type.
	/// </exception>
	public void Set(Entity entity, bool shouldHave);
}