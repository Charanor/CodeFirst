namespace CodeFirst.Utils;

/// <summary>
///     When fetching data (from any "fetchable" source) this can be used to specify how the data should be fetched.
/// </summary>
public enum FetchMode
{
	/// <summary>
	///     Fetches data from the cache first, and if the data does not exist in the cache fetches new data from the source.
	/// </summary>
	/// <remarks>
	///     This mode will write any data fetched from source to the cache
	/// </remarks>
	CacheThenSource = default,

	/// <summary>
	///     Fetches data from the source first, and if the source could not fetch the data (e.g. a REST endpoint might be
	///     unavailable or a file might not exist) it will try to read the data from the cache.
	/// </summary>
	/// <remarks>
	///     This mode will write any data fetched from source to the cache
	/// </remarks>
	SourceThenCache,

	/// <summary>
	///     Fetches data from the cache only.
	/// </summary>
	/// <remarks>
	///     This mode will not re-write any data to the cache.
	/// </remarks>
	CacheOnly,

	/// <summary>
	///     Fetches data from the source only, bypassing the cache.
	/// </summary>
	/// <remarks>
	///     This mode <b>WILL</b> write the fetched data to the cache, overriding any existing data.
	/// </remarks>
	/// <seealso cref="NoCache" />
	SourceOnly,

	/// <summary>
	///     Fetches data from the source only, bypassing the cache.
	/// </summary>
	/// <remarks>
	///     This mode <b>WILL NOT</b> write any data to the cache.
	/// </remarks>
	/// <seealso cref="SourceOnly" />
	NoCache
}