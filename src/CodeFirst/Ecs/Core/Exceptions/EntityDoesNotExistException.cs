namespace CodeFirst.Ecs.Core.Exceptions;

public class EntityDoesNotExistException : Exception
{
	public EntityDoesNotExistException(Entity entity) : base($"Entity {entity} does not exist!")
	{
	}
}