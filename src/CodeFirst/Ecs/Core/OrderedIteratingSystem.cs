namespace CodeFirst.Ecs.Core;

public abstract class OrderedIteratingSystem : IteratingSystem
{
	protected abstract int Order(Entity first);

	public override void Update(float delta)
	{
		using var handle = Entities.BeginHandle();
		foreach (var entity in handle.OrderBy(Order))
		{
			CurrentEntity = entity;
			Update(entity, delta);
			CurrentEntity = Entity.Invalid;
		}
	}
}