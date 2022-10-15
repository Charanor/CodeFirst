namespace JXS.Ecs.Core;

public class SingletonComponentMapper<T> : ComponentMapper<T> where T : ISingletonComponent, IEquatable<T>, new()
{
	private T singletonInstance;

	public SingletonComponentMapper(World world) : base(world)
	{
		singletonInstance = new T();
	}

	public ref T SingletonInstance => ref singletonInstance;

	public override bool Has(int entity) => true;

	public override ref T Get(int entity) => ref SingletonInstance;

	public override ref T Update(int entity, in T component)
	{
		SingletonInstance = component;
		return ref SingletonInstance;
	}

	public override ref T Create(int entity) => ref SingletonInstance;

	public override ref T Add(int entity, in T component) => ref SingletonInstance;

	public override void Remove(int entity)
	{
		// Noop
	}

	public override void Set(int entity, bool shouldHave)
	{
		// Noop
	}

	public override ref T AddIfMissing(int entity, in T component) => ref SingletonInstance;
}