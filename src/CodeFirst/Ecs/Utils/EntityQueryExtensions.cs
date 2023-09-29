using CodeFirst.Ecs.Core;

namespace CodeFirst.Ecs.Utils;

public static class EntityQueryExtensions
{
	/// <inheritdoc cref="EntityQuery" />
	public static EntityQuery QueryEntities(this World world) => new(world);

	/// <summary>
	///     Exclude <paramref name="entityToExclude" /> from the results of this query.
	/// </summary>
	/// <param name="query">the query</param>
	/// <param name="entityToExclude">the entity to exclude</param>
	/// <returns>the query, for chaining</returns>
	public static EntityQuery Exclude(this EntityQuery query, Entity entityToExclude) => query
		.ThatMatch((_, entity) => entity != entityToExclude);
}