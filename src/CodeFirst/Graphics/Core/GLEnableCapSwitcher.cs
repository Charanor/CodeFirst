namespace CodeFirst.Graphics.Core;

public static class GLEnableCapSwitcher
{
	private static readonly IReadOnlyList<EnableCap> AllValues = Enum.GetValues<EnableCap>();
	private static readonly int ArraySize = AllValues.Count;

	private static readonly Stack<bool[]> ArrayPool = new();
	private static readonly Stack<bool[]> States = new();

	public static void PushState()
	{
		var array = Obtain();
		for (var i = 0; i < ArraySize; i++)
		{
			array[i] = IsEnabled(AllValues[i]);
		}

		States.Push(array);
	}

	public static void Popstate()
	{
		if (!States.TryPop(out var state))
		{
			return;
		}

		for (var i = 0; i < ArraySize; i++)
		{
			var cap = AllValues[i];
			var isEnabled = state[i];
			if (isEnabled)
			{
				Enable(cap);
			}
			else
			{
				Disable(cap);
			}
		}

		Release(state);
	}

	private static bool[] Obtain() => ArrayPool.TryPop(out var arr) ? arr : new bool[ArraySize];
	private static void Release(bool[] array) => ArrayPool.Push(array);
}