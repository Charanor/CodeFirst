namespace JXS.Ecs.Core;

/// <summary>
/// A collection of flags that represents if a component is present/active or not.
/// </summary>
public record ComponentFlags
{
	private readonly bool[] componentFlags;

	public ComponentFlags(params bool[] componentFlags)
	{
		this.componentFlags = new bool[componentFlags.Length];
		Array.Copy(componentFlags, this.componentFlags, componentFlags.Length);
		Empty = !this.componentFlags.Any(flag => flag);
	}

	public bool Empty { get; }

	public override string ToString() => string.Join(", ",
		componentFlags.Where(flag => flag).Select((_, i) => ComponentManager.GetType(i).Name));

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

	public static implicit operator ComponentFlags(bool[] values) => new(values);
}