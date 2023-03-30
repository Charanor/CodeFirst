namespace JXS.Ecs.Core;

public class SingletonComponentMapper<T> : ComponentMapper<T> where T : ISingletonComponent, new()
{
	private T singletonInstance;

	public SingletonComponentMapper(World world) : base(world)
	{
		singletonInstance = new T();
	}

	public ref T SingletonInstance => ref singletonInstance;

	public override bool Has(Entity entity) => entity == Entity.SingletonEntity;

	public override ref T Get(Entity entity) => ref SingletonInstance;

	public override ref T Update(Entity entity, in T component)
	{
		SingletonInstance = component;
		return ref SingletonInstance;
	}

	public override ref T Create(Entity entity) => ref SingletonInstance;

	public override ref T Add(Entity entity, in T component) => ref SingletonInstance;

	public override void Remove(Entity entity)
	{
		// Noop
	}

	public override void Set(Entity entity, bool shouldHave)
	{
		// Noop
	}

	public override ref T AddIfMissing(Entity entity, in T component) => ref SingletonInstance;
}