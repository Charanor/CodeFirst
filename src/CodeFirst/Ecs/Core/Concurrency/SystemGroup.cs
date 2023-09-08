namespace CodeFirst.Ecs.Core.Concurrency;

public class SystemGroup : ISystemGroup
{
	private readonly ISet<Type> desiredSystems;
	private readonly IDictionary<Type, EntitySystem> systems;

	public SystemGroup()
	{
		desiredSystems = new HashSet<Type>();
		systems = new Dictionary<Type, EntitySystem>();
	}

	public bool ShouldContainSystem(Type system) => desiredSystems.Contains(system);

	public bool ShouldContainSystem<TSystem>() where TSystem : EntitySystem
		=> ShouldContainSystem(typeof(TSystem));

	public bool HasSystem(Type system) => systems.ContainsKey(system);

	public bool HasSystem<TSystem>() where TSystem : EntitySystem
		=> HasSystem(typeof(TSystem));

	internal void AddSystem<TSystem>(TSystem system) where TSystem : EntitySystem
	{
		if (!ShouldContainSystem<TSystem>())
		{
			throw new ArgumentException($"System of type {system.GetType()} does not belong to this group.",
				nameof(system));
		}
	}

	internal void SetShouldContainSystem<TSystem>() => SetShouldContainSystem(typeof(TSystem));
	internal void SetShouldContainSystem(Type system) => desiredSystems.Add(system);
}