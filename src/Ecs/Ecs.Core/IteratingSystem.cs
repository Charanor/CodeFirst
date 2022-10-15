namespace JXS.Ecs.Core;

/// <summary>
///     An <see cref="EntitySystem" /> that iterates over the entities in its <see cref="Aspect" /> one-by-one.
/// </summary>
public abstract class IteratingSystem : EntitySystem
{
	protected IteratingSystem()
	{
	}

	protected IteratingSystem(Aspect aspect, Pass pass) : base(aspect, pass)
	{
	}

	protected abstract void Update(int entity, float delta);

	public override void Update(float delta)
	{
		var (entities, size) = Entities.Begin();

		for (var i = 0; i < size; i++)
		{
			var entity = entities[i];
			Update(entity, delta);
		}

		Entities.Commit();
	}
}