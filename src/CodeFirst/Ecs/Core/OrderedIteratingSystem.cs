using CodeFirst.Utils.Collections;

namespace CodeFirst.Ecs.Core;

public abstract class OrderedIteratingSystem : IteratingSystem
{
	private readonly SnapshotList<Entity> entityCache = new();
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
		var (array, size) = entityCache.Begin();
		{
			var index = Array.BinarySearch(array, index: 0, size, entity, entityComparer);
			if (index < 0)
			{
				// From MSDN: Return Value: The zero-based index of item in the sorted List<T>, if item is found;
				// otherwise, a negative number that is the bitwise complement of the index of the next element that is
				// larger than item or, if there is no larger element, the bitwise complement of Count.
				index = ~index;
			}

			entityCache.Insert(index, entity);
		}
		entityCache.Commit();
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