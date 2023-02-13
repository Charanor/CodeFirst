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

	protected Entity CurrentEntity { get; set; }

	protected abstract void Update(Entity entity, float delta);

	public override void Update(float delta)
	{
		var (entities, size) = Entities.Begin();

		for (var i = 0; i < size; i++)
		{
			var entity = entities[i];
			CurrentEntity = entity;
			Update(entity, delta);
			CurrentEntity = Entity.Invalid;
		}

		Entities.Commit();
	}

	protected void AssertHasEntity(string methodName)
	{
		if (!CurrentEntity.IsValid)
		{
			throw new InvalidOperationException($"Can not call {methodName} while not processing an entity!");
		}
	}

	protected sealed override void Remove(Entity entity)
	{
		AssertHasEntity(nameof(Remove));
		base.Remove(entity);
	}

	protected void RemoveEntity()
	{
		AssertHasEntity(nameof(RemoveEntity));
		base.Remove(CurrentEntity);
	}
}