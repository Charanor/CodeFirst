using System.Collections;

namespace CodeFirst.Utils.Collections;

public sealed class SnapshotListIterationHandle<T> : IDisposable, IEnumerable<T>
{
	private readonly IReadOnlySnapshotList<T> list;
	private readonly HandleAction defaultAction;
	private readonly T[] items;
	private readonly int count;

	private bool hasEnded;

	public SnapshotListIterationHandle(IReadOnlySnapshotList<T> list, HandleAction defaultAction = HandleAction.Discard)
	{
		this.list = list;
		this.defaultAction = defaultAction;
		list.Begin(out items, out count);
		hasEnded = false;
	}

	public void Dispose()
	{
		switch (defaultAction)
		{
			case HandleAction.Discard:
				Discard();
				break;
			case HandleAction.Commit:
				Commit();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (var i = 0; i < count; i++)
		{
			yield return items[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Discard()
	{
		if (hasEnded)
		{
			return;
		}

		hasEnded = true;
		list.Discard();
	}

	public void Commit()
	{
		if (hasEnded)
		{
			return;
		}

		hasEnded = true;
		list.Commit();
	}

	public void Deconstruct(out T[] outItems, out int outCount)
	{
		outItems = items;
		outCount = count;
	}
}