using System.Collections;
using System.Runtime.CompilerServices;
using CodeFirst.Ecs.Core;
using CodeFirst.Ecs.Core.Exceptions;
using CodeFirst.Utils.Collections;
using CodeFirst.Utils.Events;
using OpenTK.Mathematics;

namespace CodeFirst.Ecs.Utils;

/// <summary>
///     A list implementation that guaranteed that no add/remove operations will happen inside a <see cref="Begin()" />{
///     ... }[<see cref="Commit" /> / <see cref="Discard" />] block. Any changes will be applied (if ending with
///     <see cref="Commit" />) or discarded (if ending with <see cref="Discard" />) after the respective methods are
///     called, not before.
/// </summary>
/// <example>
///     var (snapshot, size) = MyList.Begin();<br />
///     <br />
///     for(var i = 0; i &lt; size; i++) { var item = snapshot[i]; } <br />
///     <br />
///     MyList.Commit();<br />
///     // Or: MyList.Discard();
/// </example>
public class EntitySnapshotList : ISnapshotList<Entity>
{
	private Entity[] backingArray;
	private Entity[] iteratingArray;

	private bool modified;

	public EntitySnapshotList()
	{
		backingArray = new Entity[128];
		iteratingArray = Array.Empty<Entity>();
	}

	/// <summary>
	///     The size of the list. This will return the actual size of the list, not the size of the array being iterated over
	///     inside of a Begin/Commit block. This size will update immediately and will not wait for Commit/Discard to be
	///     called.
	/// </summary>
	public int Count => backingArray.Length;

	public Entity this[int index]
	{
		get => EntityAtIndex(index);
		set
		{
			Validate(value);

			if (index != value.Id)
			{
				throw new IndexOutOfRangeException(
					"An entity's index in an EntitySnapshotList should be the same as its Id");
			}

			Modified();
			EnsureSize(index);
			var prevItem = EntityAtIndex(index);
			backingArray[index] = value;

			if (prevItem.IsValid)
			{
				OnItemRemoved?.Invoke(this, new EventArgs<Entity>(prevItem));
			}

			OnItemAdded?.Invoke(this, new EventArgs<Entity>(value));
		}
	}

	/// <summary>
	///     Adds an item to this list. Will not be added to the iterating array inside of a Begin/Commit block until
	///     Commit is called. If not inside a Begin/Commit block, the item will be added immediately as usual.
	/// </summary>
	/// <param name="item">the item to add</param>
	/// <exception cref="IndexOutOfRangeException">If an entity with the same id as <paramref name="item" /> exists.</exception>
	public void Add(Entity item)
	{
		Validate(item);
		EnsureSize(item.Id);

		var oldItem = backingArray[item.Id];
		if (oldItem.IsValid)
		{
			throw new IndexOutOfRangeException(
				$"{nameof(EntitySnapshotList)} already contains an entry for entity {item}");
		}

		Modified();
		backingArray[item.Id] = item;
		OnItemAdded?.Invoke(this, new EventArgs<Entity>(item));
	}

	/// <summary>
	///     Inserts an item into this list. Will not be inserted into the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be inserted immediately as usual.
	/// </summary>
	/// <param name="index">the index to insert the item into</param>
	/// <param name="item">the item to add</param>
	public void Insert(int index, Entity item)
	{
		Validate(item);
		EnsureSize(index);
		if (index != item.Id)
		{
			throw new IndexOutOfRangeException(
				"An entity's index in an EntitySnapshotList should be the same as its Id");
		}

		Modified();
		var prevItem = EntityAtIndex(index);
		backingArray[index] = item;

		if (prevItem.IsValid)
		{
			OnItemRemoved?.Invoke(this, new EventArgs<Entity>(prevItem));
		}

		OnItemAdded?.Invoke(this, new EventArgs<Entity>(item));
	}

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="index">the index to remove</param>
	public void RemoveAt(int index)
	{
		Modified();
		EnsureSize(index);
		var item = EntityAtIndex(index);
		backingArray[index] = Entity.Invalid;

		if (item.IsValid)
		{
			OnItemRemoved?.Invoke(this, new EventArgs<Entity>(item));
		}
	}

	/// <summary>
	///     Removes all items from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the items will be removed immediately as usual.
	/// </summary>
	public void Clear()
	{
		Modified();
		for (var i = 0; i < backingArray.Length; i++)
		{
			var item = backingArray[i];
			if (item.IsValid)
			{
				OnItemRemoved?.Invoke(this, new EventArgs<Entity>(item));
			}

			backingArray[i] = Entity.Invalid;
		}
	}

	/// <summary>
	///     Copies this list to the given array.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, and will also copy any items added inside the block
	///     before Commit was called.
	/// </remarks>
	public void CopyTo(Entity[] array, int arrayIndex)
	{
		backingArray.CopyTo(array, arrayIndex);
	}

	/// <summary>
	///     Get the index of the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block can
	///     not be indexed by this method.
	/// </remarks>
	public int IndexOf(Entity item) => item.Id < 0 || item.Id >= backingArray.Length ? -1 : item.Id;

	/// <summary>
	///     Checks if this list contains the given item.
	/// </summary>
	/// <remarks>
	///     Note that this does not respect the Begin/Commit block, so items that were removed inside of the block will
	///     return <c>false</c> for this method, and items added will return <c>true</c>.
	/// </remarks>
	public bool Contains(Entity item) => item.IsValid && item.Id < backingArray.Length && backingArray[item.Id].IsValid;

	public IEnumerator<Entity> GetEnumerator()
	{
		if (IsIterating)
		{
			throw new InvalidOperationException(
				$"Cannot iterate over a {nameof(EntitySnapshotList)} that is already iterating!");
		}

		// ReSharper disable once ForCanBeConvertedToForeach
		for (var i = 0; i < backingArray.Length; i++)
		{
			var entity = backingArray[i];
			if (!entity.IsValid)
			{
				continue;
			}

			yield return entity;
		}
	}

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
	public event EventHandler<IReadOnlySnapshotList<Entity>, EventArgs<Entity>>? OnItemAdded;

	/// <summary>
	///     Called when an item is removed from this list.
	/// </summary>
	/// <remarks>
	///     This will be called immediately when an item is removed, even if currently iterating over this list, and will
	///     not respect a call to <see cref="Discard" />
	/// </remarks>
	public event EventHandler<IReadOnlySnapshotList<Entity>, EventArgs<Entity>>? OnItemRemoved;

	/// <summary>
	///     Removes an item from this list. Will not be removed from the iterating array inside of a Begin/Commit block
	///     until Commit is called. If not inside a Begin/Commit block, the item will be removed immediately as usual.
	/// </summary>
	/// <param name="item">the item to remove</param>
	public void Remove(Entity item)
	{
		Validate(item);
		if (item.Id < 0 || item.Id >= backingArray.Length)
		{
			return;
		}

		Modified();
		backingArray[item.Id] = Entity.Invalid;
		OnItemRemoved?.Invoke(this, new EventArgs<Entity>(item));
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
	public (Entity[] iteratingArray, int size) Begin()
	{
		if (IsIterating)
		{
			throw new InvalidOperationException(
				$"Can not {nameof(Begin)}() twice simultaneously on a {nameof(EntitySnapshotList)}. Call {nameof(Commit)} or {nameof(Discard)} first.");
		}

		if (iteratingArray.Length < Count || iteratingArray.Length >= Count * 2)
		{
			Array.Resize(ref iteratingArray, Count);
		}

		var validEntityCount = 0;
		for (var i = 0; i < backingArray.Length; i++)
		{
			var entity = EntityAtIndex(i);
			if (!entity.IsValid)
			{
				continue;
			}

			iteratingArray[validEntityCount] = entity;
			validEntityCount += 1;
		}

		IsIterating = true;
		modified = false;
		return (iteratingArray, validEntityCount);
	}

	/// <inheritdoc cref="Begin()" />
	/// <returns></returns>
	/// <param name="outputIteratingArray">the safely iterable array</param>
	/// <param name="size">the size of the safely iterable array</param>
	public void Begin(out Entity[] outputIteratingArray, out int size)
	{
		(outputIteratingArray, size) = Begin();
	}

	public SnapshotListIterationHandle<Entity> BeginHandle(HandleAction defaultAction = HandleAction.Discard) =>
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
				$"Can not call {nameof(Commit)}() on a {nameof(EntitySnapshotList)} that is not iterating. Call {nameof(Begin)} first.");
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
				$"Can not call {nameof(Discard)}() on a {nameof(EntitySnapshotList)} that is not iterating. Call {nameof(Begin)} first.");
		}

		if (modified)
		{
			Array.Copy(iteratingArray, backingArray, iteratingArray.Length);
			for (var i = iteratingArray.Length; i < backingArray.Length; i++)
			{
				backingArray[i] = Entity.Invalid;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Entity EntityAtIndex(int index) =>
		index < 0 || index >= backingArray.Length ? Entity.Invalid : backingArray[index];

	private void EnsureSize(int containedIndex)
	{
		var arrayLength = backingArray.Length;
		if (arrayLength > containedIndex)
		{
			// Already contains this index
			return;
		}

		var newSize = MathHelper.NextPowerOfTwo(containedIndex + 1);
		Array.Resize(ref backingArray, newSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Validate(Entity entity)
	{
		if (!entity.IsValid)
		{
			throw new InvalidEntityException($"Cannot interface an invalid entity with {nameof(EntitySnapshotList)}");
		}
	}
}