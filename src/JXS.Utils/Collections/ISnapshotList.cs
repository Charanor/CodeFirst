namespace JXS.Utils.Collections;

public interface ISnapshotList<T> : IReadOnlySnapshotList<T>
{
	new T this[int index] { get; set; }

	/// <summary>
	///     Adds an item to this list. Will not be added to the iterating array inside of a Begin/Commit block until
	///     Commit is called. If not inside a Begin/Commit block, the item will be added immediately as usual.
	/// </summary>
	/// <param name="item">the item to add</param>
	void Add(T item);

	/// <summary>
	///     Inserts an item into this list. Will not be inserted into the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be inserted immediately as usual.
	/// </summary>
	/// <param name="index">the index to insert the item into</param>
	/// <param name="item">the item to add</param>
	void Insert(int index, T item);

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="index">the index to remove</param>
	void RemoveAt(int index);

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="item">the item to remove</param>
	void Remove(T item);

	/// <summary>
	///     Removes all items from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the items will be removed immediately as usual.
	/// </summary>
	void Clear();

	/// <summary>
	///     Copies this list to the given array.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, and will also copy any items added inside the block
	///     before Commit was called.
	/// </remarks>
	void CopyTo(T[] array, int arrayIndex);
}