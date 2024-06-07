using System.Collections;
using CodeFirst.Utils.Events;
using JetBrains.Annotations;

namespace CodeFirst.Utils.Collections;

/// <summary>
///     A list implementation that guarantees that no add/remove operations will happen inside a <see cref="Begin()" />{
///     ... }[<see cref="Commit" /> / <see cref="Discard" />] block. Any changes will be applied (if ending with
///     <see cref="Commit" />) or discarded (if ending with <see cref="Discard" />) after the respective methods are
///     called, not before.
/// </summary>
/// <example>
///     var (snapshot, size) = MyList.Begin();<br />
///     <br />
///     for(var i = 0; i &lt; size; i++) { var item = snapshot[i]; } <br />
///     <br />
///     MyList.Commit(); // Or: MyList.Discard();<br />
///		// Can also be used with foreach like so: <br /><br />
///		var handle = MyList.BeginHandle();<br />
///		foreach (var entity in handle) { ... }
/// </example>
/// <typeparam name="T">the item type</typeparam>
[PublicAPI]
public class SnapshotList<T> : IList<T>, ISnapshotList<T>
{
	private readonly IList<T> backingList;
	private T[] iteratingArray;

	private bool modified;

	public SnapshotList()
	{
		backingList = new List<T>();
		iteratingArray = Array.Empty<T>();
	}

	/// <summary>
	///     The size of the list. This will return the actual size of the list, not the size of the array being iterated over
	///     inside of a Begin/Commit block. This size will update immediately and will not wait for Commit/Discard to be
	///     called.
	/// </summary>
	public int Count => backingList.Count;

	/// <value>false</value>
	public bool IsReadOnly => false;

	public T this[int index]
	{
		get => backingList[index];
		set
		{
			Modified();
			var prevItem = backingList[index];
			backingList[index] = value;

			if (prevItem is not null)
			{
				OnItemRemoved?.Invoke(this, new EventArgs<T>(prevItem));
			}

			if (value is not null)
			{
				OnItemAdded?.Invoke(this, new EventArgs<T>(value));
			}
		}
	}

	/// <summary>
	///     Adds an item to this list. Will not be added to the iterating array inside of a Begin/Commit block until
	///     Commit is called. If not inside a Begin/Commit block, the item will be added immediately as usual.
	/// </summary>
	/// <param name="item">the item to add</param>
	public void Add(T item)
	{
		Modified();
		backingList.Add(item);

		if (item is not null)
		{
			OnItemAdded?.Invoke(this, new EventArgs<T>(item));
		}
	}

	/// <summary>
	///     Inserts an item into this list. Will not be inserted into the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be inserted immediately as usual.
	/// </summary>
	/// <param name="index">the index to insert the item into</param>
	/// <param name="item">the item to add</param>
	public void Insert(int index, T item)
	{
		Modified();
		var prevItem = index < 0 || index >= backingList.Count ? default : backingList[index];
		backingList.Insert(index, item);

		if (prevItem is not null)
		{
			OnItemRemoved?.Invoke(this, new EventArgs<T>(prevItem));
		}

		if (item is not null)
		{
			OnItemAdded?.Invoke(this, new EventArgs<T>(item));
		}
	}

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="index">the index to remove</param>
	public void RemoveAt(int index)
	{
		Modified();
		var item = backingList[index];
		backingList.RemoveAt(index);

		if (item is not null)
		{
			OnItemRemoved?.Invoke(this, new EventArgs<T>(item));
		}
	}

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="item">the item to remove</param>
	bool ICollection<T>.Remove(T item)
	{
		Modified();
		var wasRemoved = backingList.Remove(item);

		if (wasRemoved && item is not null)
		{
			OnItemRemoved?.Invoke(this, new EventArgs<T>(item));
		}

		return wasRemoved;
	}

	/// <summary>
	///     Removes all items from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the items will be removed immediately as usual.
	/// </summary>
	public void Clear()
	{
		Modified();
		foreach (var item in backingList)
		{
			if (item is not null)
			{
				OnItemRemoved?.Invoke(this, new EventArgs<T>(item));
			}
		}

		backingList.Clear();
	}

	/// <summary>
	///     Copies this list to the given array.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, and will also copy any items added inside the block
	///     before Commit was called.
	/// </remarks>
	public void CopyTo(T[] array, int arrayIndex)
	{
		backingList.CopyTo(array, arrayIndex);
	}

	/// <summary>
	///     Get the index of the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block can
	///     not be indexed by this method.
	/// </remarks>
	public int IndexOf(T item) => backingList.IndexOf(item);


	/// <summary>
	///     Checks if this list contains the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block will
	///     return <c>false</c> for this method, and items added will return <c>true</c>.
	/// </remarks>
	public bool Contains(T item) => backingList.Contains(item);

	public IEnumerator<T> GetEnumerator() => backingList.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	///     If the list is currently being iterated inside of a Begin/Commit block.
	/// </summary>
	public bool IsIterating { get; private set; }

	/// <summary>
	///     Called when an item is added to this list.
	/// </summary>
	/// <remarks>
	///     This will be called immediately when an item is added, even if currently iterating over this list, and will
	///     not respect a call to <see cref="Discard" />
	/// </remarks>
	public event EventHandler<IReadOnlySnapshotList<T>, EventArgs<T>>? OnItemAdded;

	/// <summary>
	///     Called when an item is removed from this list.
	/// </summary>
	/// <remarks>
	///     This will be called immediately when an item is removed, even if currently iterating over this list, and will
	///     not respect a call to <see cref="Discard" />
	/// </remarks>
	public event EventHandler<IReadOnlySnapshotList<T>, EventArgs<T>>? OnItemRemoved;

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="item">the item to remove</param>
	public void Remove(T item)
	{
		if (!backingList.Remove(item))
		{
			return;
		}

		Modified();
		OnItemRemoved?.Invoke(this, new EventArgs<T>(item));
	}

	/// <summary>
	///     Begins safe iteration over this list. The returned array is guaranteed to not change until a call to
	///     <see cref="Commit" /> or <see cref="Discard" />.
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
	public (T[] iteratingArray, int size) Begin()
	{
		if (IsIterating)
		{
			throw new InvalidOperationException(
				$"Can not {nameof(Begin)}() twice simultaneously on a {nameof(SnapshotList<T>)}. Call {nameof(Commit)} or {nameof(Discard)} first.");
		}

		if (iteratingArray.Length < Count || iteratingArray.Length >= Count * 2)
		{
			Array.Resize(ref iteratingArray, Count);
		}

		for (var i = 0; i < Count; i++)
		{
			iteratingArray[i] = backingList[i];
		}

		IsIterating = true;
		modified = false;
		return (iteratingArray, Count);
	}

	/// <inheritdoc cref="Begin()" />
	/// <returns></returns>
	/// <param name="outputIteratingArray">the safely iterable array</param>
	/// <param name="size">the size of the safely iterable array</param>
	public void Begin(out T[] outputIteratingArray, out int size)
	{
		(outputIteratingArray, size) = Begin();
	}

	public SnapshotListIterationHandle<T> BeginHandle(HandleAction defaultAction = HandleAction.Discard) =>
		new(this, defaultAction);

	/// <summary>
	///     Commits the changes made inside of a Begin/Commit block and ends iteration.
	/// </summary>
	/// <exception cref="InvalidOperationException">If not called inside of a Begin/Commit block</exception>
	public void Commit()
	{
		if (!IsIterating)
		{
			throw new InvalidOperationException(
				$"Can not call {nameof(Commit)}() on a {nameof(SnapshotList<T>)} that is not iterating. Call {nameof(Begin)} first.");
		}

		IsIterating = false;
	}

	/// <summary>
	///     Discards the changes made inside of a Begin/Discard block and ends iteration. This is a slower operation
	///     than <see cref="Commit" />, because the changes have to be manually reverted.
	/// </summary>
	/// <exception cref="InvalidOperationException">If not called inside of a Begin/Discard block</exception>
	public void Discard()
	{
		if (!IsIterating)
		{
			throw new InvalidOperationException(
				$"Can not call {nameof(Discard)}() on a {nameof(SnapshotList<T>)} that is not iterating. Call {nameof(Begin)} first.");
		}

		if (modified)
		{
			backingList.Clear();
			for (var i = 0; i < iteratingArray.Length; i++)
			{
				backingList[i] = iteratingArray[i];
			}
		}

		IsIterating = false;
	}

	private void Modified()
	{
		if (IsIterating)
		{
			modified = true;
		}
	}
}