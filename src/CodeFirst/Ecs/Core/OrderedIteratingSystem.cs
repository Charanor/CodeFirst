using System.Diagnostics;
using CodeFirst.Ecs.Utils;
using CodeFirst.Utils.Collections;

namespace CodeFirst.Ecs.Core;

public abstract class OrderedIteratingSystem : IteratingSystem
{
	private readonly EntitySnapshotList entityCache = new();
	private IComparer<Entity> entityComparer = null!;

	protected abstract IComparer<Entity> CreateComparer();

	public override void Initialize(World world)
	{
		base.Initialize(world);
		entityComparer = CreateComparer();
		
		foreach (var entity in Entities.Order(entityComparer))
		{
			entityCache.Add(entity);
		}
	}

	protected override void EntityAdded(Entity entity)
	{
		base.EntityAdded(entity);
		entityCache.Add(entity);
	}

	protected override void EntityRemoved(Entity entity)
	{
		base.EntityRemoved(entity);
		entityCache.Remove(entity);
	}

	public override void Update(float delta)
	{
		using var handle = entityCache.BeginHandle();
		foreach (var entity in handle)
		{
			CurrentEntity = entity;
			Update(entity, delta);
			CurrentEntity = Entity.Invalid;
		}
	}
}