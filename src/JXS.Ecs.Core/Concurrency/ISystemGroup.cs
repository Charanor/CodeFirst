namespace JXS.Ecs.Core.Concurrency;

public interface ISystemGroup
{
	bool ShouldContainSystem<TSystem>() where TSystem : EntitySystem;
	bool ShouldContainSystem(Type system);

	bool HasSystem<TSystem>() where TSystem : EntitySystem;
	bool HasSystem(Type system);
}