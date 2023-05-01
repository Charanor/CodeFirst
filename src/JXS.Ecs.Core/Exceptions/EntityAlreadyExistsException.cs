namespace JXS.Ecs.Core.Exceptions;

public class EntityAlreadyExistsException : Exception
{
	public EntityAlreadyExistsException(Entity entity) : base($"Entity {entity} already exists!")
	{
	}
}