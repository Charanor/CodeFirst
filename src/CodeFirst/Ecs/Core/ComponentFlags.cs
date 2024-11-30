namespace CodeFirst.Ecs.Core;

/// <summary>
///     A collection of flags that represents if a component is present/active or not.
/// </summary>
public record ComponentFlags
{
	public static readonly ComponentFlags Empty = new();

	private readonly bool[] componentFlags;

	public ComponentFlags(params bool[] componentFlags)
	{
		this.componentFlags = new bool[componentFlags.Length];
		Array.Copy(componentFlags, this.componentFlags, componentFlags.Length);
		IsEmpty = !this.componentFlags.Any(flag => flag);
	}

	public bool IsEmpty { get; }

	public override string ToString() => string.Join(", ", componentFlags
		.Select((flag, i) => flag ? ComponentManager.GetType(i).Name : string.Empty)
		.Where(str => str != string.Empty));

	public bool Has(int componentId)
	{
		if (componentId < 0 || componentId >= componentFlags.Length)
		{
			return false;
		}

		return componentFlags[componentId];
	}

	public bool ContainsAll(ComponentFlags other)
	{
		for (var i = 0; i < other.componentFlags.Length; i++)
		{
			var otherHasFlag = other.componentFlags[i];
			var thisHasFlag = i >= 0 && i < componentFlags.Length && componentFlags[i];

			// If "other" has a flag, and we don't have the flag, we don't match.
			// (The other way around is fine though)
			if (otherHasFlag && !thisHasFlag)
			{
				return false;
			}
		}

		return true;
	}

	public bool ContainsSome(ComponentFlags other)
	{
		var maxIdx = Math.Min(other.componentFlags.Length, componentFlags.Length);
		for (var i = 0; i < maxIdx; i++)
		{
			var otherHasFlag = other.componentFlags[i];
			var thisHasFlag = componentFlags[i];

			// If "other" has a flag, and we also have the flag, we match
			if (otherHasFlag && thisHasFlag)
			{
				return true;
			}
		}

		return false;
	}

	public bool ContainsNone(ComponentFlags other) => !ContainsSome(other);

	public IEnumerable<int> GetComponentIds() => componentFlags
		.Select((flag, i) => new { Flag = flag, Index = i })
		.Where(item => item.Flag)
		.Select(item => item.Index);

	public static implicit operator ComponentFlags(bool[] values) => new(values);

	public static ComponentFlags operator &(ComponentFlags left, ComponentFlags right)
	{
		var flagCount = Math.Max(left.componentFlags.Length, right.componentFlags.Length);
		var flags = new bool[flagCount];
		for (var i = 0; i < flagCount; i++)
		{
			var f1 = i < left.componentFlags.Length && left.componentFlags[i];
			var f2 = i < right.componentFlags.Length && right.componentFlags[i];
			flags[i] = f1 && f2;
		}

		return flags;
	}

	public static ComponentFlags operator |(ComponentFlags left, ComponentFlags right)
	{
		var flagCount = Math.Max(left.componentFlags.Length, right.componentFlags.Length);
		var flags = new bool[flagCount];
		for (var i = 0; i < flagCount; i++)
		{
			var f1 = i < left.componentFlags.Length && left.componentFlags[i];
			var f2 = i < right.componentFlags.Length && right.componentFlags[i];
			flags[i] = f1 || f2;
		}

		return flags;
	}
}