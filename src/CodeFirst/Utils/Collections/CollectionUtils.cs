namespace CodeFirst.Utils.Collections;

public static class CollectionUtils
{
	/// <summary>
	///     Clears the list and then calls <see cref="List{T}.AddRange" />
	/// </summary>
	/// <param name="list">the list to perform the operation on</param>
	/// <param name="items">the items to replace the contents of this list with</param>
	/// <typeparam name="T">the item type of the list</typeparam>
	public static void ReplaceWithRange<T>(this List<T> list, IEnumerable<T> items)
	{
		list.Clear();
		list.AddRange(items);
	}
}