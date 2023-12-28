using CodeFirst.Ecs.Core;

namespace CodeFirst.Ecs.Utils;

public class EntityDictionary<T> where T : class
{
	private T[] items;
	private bool[] hasKey;

	public EntityDictionary(int defaultSize = 128)
	{
		items = new T[defaultSize];
		hasKey = new bool[defaultSize];
	}

	public void Add(Entity entity, T item)
	{
		if (!entity.IsValid)
		{
			return;
		}

		CreateSpace(entity);
		items[entity.Id] = item;
		hasKey[entity.Id] = true;
	}

	public void Remove(Entity entity)
	{
		if (!entity.IsValid)
		{
			return;
		}

		CreateSpace(entity);
		hasKey[entity.Id] = false;
	}

	public bool Has(Entity entity) => entity.IsValid && entity.Id < hasKey.Length && hasKey[entity.Id];
	public T Get(Entity entity) => Has(entity) ? items[entity.Id] : throw new IndexOutOfRangeException();

	private void CreateSpace(Entity entity)
	{
		if (entity.Id < items.Length)
		{
			return;
		}

		var newSize = Math.Max(items.Length * 2, entity.Id + 1);
		Array.Resize(ref items, newSize);
		Array.Resize(ref hasKey, newSize);
	}
}