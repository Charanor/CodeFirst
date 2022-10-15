namespace JXS.Ecs.Core;

public interface IAspect
{
	/// <summary>
	///     Checks if the <paramref name="entity" /> matches this IAspect.
	/// </summary>
	/// <param name="world">the world containing the entity</param>
	/// <param name="entity"></param>
	/// <returns>true if the entity matches, false otherwise</returns>
	bool Matches(World world, int entity);
}