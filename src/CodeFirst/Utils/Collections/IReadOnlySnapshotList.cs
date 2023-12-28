using CodeFirst.Utils.Events;

namespace CodeFirst.Utils.Collections;

public interface IReadOnlySnapshotList<T> : IEnumerable<T>
{
	T this[int index] { get; }

	/// <summary>
	///     If the list is currently being iterated inside of a Begin/Commit block.
	/// </summary>
	bool IsIterating { get; }

	/// <summary>
	///     The size of the list. This will return the actual size of the list, not the size of the array being iterated over
	///     inside of a Begin/Commit block. This size will update immediately and will not wait for Commit/Discard to be
	///     called.
	/// </summary>
	int Count { get; }

	/// <summary>
	///     Get the index of the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block can
	///     not be indexed by this method.
	/// </remarks>
	int IndexOf(T item);

	/// <summary>
	///     Checks if this list contains the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block will
	///     return <c>false</c> for this method, and items added will return <c>true</c>.
	/// </remarks>
	bool Contains(T item);

	/// <summary>
	///     Called when an item is added to this list.
	/// </summary>
	/// <remarks>
	///     This will be called immediately when an item is added, even if currently iterating over this list, and will
	///     not respect a call to <see cref="SnapshotList{T}.Discard" />
	/// </remarks>
	event EventHandler<IReadOnlySnapshotList<T>, EventArgs<T>>? OnItemAdded;

	/// <summary>
	///     Called when an item is removed from this list.
	/// </summary>
	/// <remarks>
	///     This will be called immediately when an item is removed, even if currently iterating over this list, and will
	///     not respect a call to <see cref="SnapshotList{T}.Discard" />
	/// </remarks>
	event EventHandler<IReadOnlySnapshotList<T>, EventArgs<T>>? OnItemRemoved;

	/// <summary>
	///     Begins safe iteration over this list. The returned array is guaranteed to not change until a call to
	///     <see cref="SnapshotList{T}.Commit" /> or <see cref="SnapshotList{T}.Discard" />.
	/// </summary>
	/// <returns>A tuple <c>(T[] iteratingArray, int size)</c> containing the safely iterable array and the size of the array.</returns>
	/// <exception cref="InvalidOperationException">If this list is already being iterated over inside a Begin/Commit block.</exception>
	/// <remarks>
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 Do <b>not</b> save a reference to <c>iteratingArray</c>. The array will be re-used in future calls to
	///                 Begin.
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 Do <b>not</b> use <c>iteratingArray.Length</c> to determine the size of the array. The array is not
	///                 guaranteed to exactly fit the items inside. Use the given <c>size</c> instead.
	///             </description>
	///         </item>
	///     </list>
	/// </remarks>
	(T[] iteratingArray, int size) Begin();

	/// <inheritdoc cref="SnapshotList{T}.Begin()" />
	/// <returns></returns>
	/// <param name="outputIteratingArray">the safely iterable array</param>
	/// <param name="size">the size of the safely iterable array</param>
	void Begin(out T[] outputIteratingArray, out int size);

	SnapshotListIterationHandle<T> BeginHandle(HandleAction defaultAction = HandleAction.Discard);

	/// <summary>
	///     Commits the changes made inside of a Begin/Commit block and ends iteration.
	/// </summary>
	/// <exception cref="InvalidOperationException">If not called inside of a Begin/Commit block</exception>
	void Commit();

	/// <summary>
	///     Discards the changes made inside of a Begin/Discard block and ends iteration. This is a slower operation
	///     than <see cref="SnapshotList{T}.Commit" />, because the changes have to be manually reverted.
	/// </summary>
	/// <exception cref="InvalidOperationException">If not called inside of a Begin/Discard block</exception>
	void Discard();
}