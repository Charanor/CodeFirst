using JetBrains.Annotations;

namespace CodeFirst.AssetManagement;

/// <summary>
///		Marks the given class as a resource that can be precached. This allows it to be precached when the
///		<see cref="Assets.PrecacheAllResources">Assets.PrecacheAllResources</see> method is called.
/// </summary>
[PublicAPI]
public interface IPrecachedResource
{
	/// <summary>
	///		When this method is called you should precache resources that are relevant to the implementing object
	///		by using the static methods contained in the <see cref="Assets"/> class.
	/// </summary>
	static abstract void Precache();
}