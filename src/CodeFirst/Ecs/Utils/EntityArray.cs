using System.Collections;
using CodeFirst.Ecs.Core;

namespace CodeFirst.Ecs.Utils;

public class EntityArray : IEnumerable<Entity>
{
	private readonly BitArray hasEntity;

	public EntityArray(int defaultSize = 128)
	{
		hasEntity = new BitArray(defaultSize);
	}

	public IEnumerator<Entity> GetEnumerator()
	{
		for (var i = 0; i < hasEntity.Count; i++)
		{
			if (hasEntity[i])
			{
				yield return new Entity(i);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Add(Entity entity)
	{
		if (!entity.IsValid)
		{
			return;
		}

		EnsureSize(entity);
		hasEntity.Set(entity.Id, value: true);
	}

	public void Remove(Entity entity)
	{
		if (!entity.IsValid)
		{
			return;
		}

		if (entity.Id >= hasEntity.Length)
		{
			return;
		}

		hasEntity.Set(entity.Id, value: false);
	}

	public bool Has(Entity entity) => entity.IsValid && Has(entity.Id);

	public bool Has(int index) => index >= 0 && index < hasEntity.Length && hasEntity[index];
	public Entity Get(int index) => !Has(index) ? Entity.Invalid : new Entity(index);

	public void Clear() => hasEntity.SetAll(false);

	private void EnsureSize(Entity entity)
	{
		if (entity.Id >= hasEntity.Length)
		{
			hasEntity.Length = Math.Max(entity.Id + 1, hasEntity.Length * 2);
		}
	}
}