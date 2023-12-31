namespace CodeFirst.Input.Actions;

public record MultiInputAction : InputAction
{
	private readonly List<InputAction> actions;

	public MultiInputAction()
	{
		actions = new List<InputAction>();
	}

	public MultiInputAction(IEnumerable<InputAction> actions) : this()
	{
		this.actions.AddRange(actions);
	}

	public MultiInputAction(params InputAction[] actions) : this((IEnumerable<InputAction>)actions)
	{
	}

	protected override float InternalValue { get; set; }
	
	internal override void OnInput(InputManager manager, InputEvent e)
	{
		if (actions.Count == 0)
		{
			return;
		}
		
		foreach (var action in actions)
		{
			action.OnInput(manager, e);
		}

		var max = 0f;
		var min = 0f;
		foreach (var action in actions)
		{
			if (action.Value > max)
			{
				max = action.Value;
			}

			if (action.Value < min)
			{
				min = action.Value;
			}
		}

		InternalValue = max + min;
	}

	public override MultiInputAction Add(InputAction action)
	{
		if (action == this)
		{
			return this;
		}

		if (action is MultiInputAction multi)
		{
			return new MultiInputAction(actions.Concat(multi.actions));
		}

		return new MultiInputAction(actions.Append(action));
	}

	public override MultiInputAction Remove(InputAction action)
	{
		if (action == this)
		{
			return this;
		}

		if (action is MultiInputAction multi)
		{
			return new MultiInputAction(actions.Except(multi.actions).Where(a => a != multi));
		}

		return new MultiInputAction(actions.Where(a => a != action));
	}

	public static MultiInputAction operator +(MultiInputAction multi, InputAction action) => multi.Add(action);
	public static MultiInputAction operator -(MultiInputAction multi, InputAction action) => multi.Remove(action);
}