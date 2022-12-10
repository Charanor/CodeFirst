using System.Collections.Immutable;

namespace JXS.Input.Core;

public class InputManager<TInputState> where TInputState : struct, Enum
{
	private readonly IDictionary<TInputState, HashSet<InputSystem>> systemMap;

	public InputManager(IInputProvider inputProvider)
	{
		InputProvider = inputProvider;
		systemMap = new Dictionary<TInputState, HashSet<InputSystem>>();
	}

	public IInputProvider InputProvider { get; }

	public TInputState State { get; set; }

	public void RegisterInputSystemForStates(InputSystem inputSystem, TInputState firstState,
		params TInputState[] states)
	{
		RegisterForState(firstState);
		foreach (var state in states)
		{
			RegisterForState(state);
		}

		void RegisterForState(TInputState state)
		{
			if (!systemMap.TryGetValue(state, out var inputSystems))
			{
				inputSystems = new HashSet<InputSystem>();
				systemMap.Add(state, inputSystems);
			}

			inputSystems.Add(inputSystem);
		}
	}

	public bool UnregisterInputSystemFromState(TInputState state, InputSystem inputSystem) =>
		systemMap.TryGetValue(state, out var inputSystems) && inputSystems.Remove(inputSystem);

	public IEnumerable<InputSystem> GetSystemsForState(TInputState state)
	{
		if (systemMap.TryGetValue(state, out var systems))
		{
			return systems;
		}

		return ImmutableHashSet<InputSystem>.Empty;
	}

	public void Update(float delta)
	{
		foreach (var (state, inputSystems) in systemMap)
		{
			foreach (var inputSystem in inputSystems)
			{
				inputSystem.Enabled = State.HasFlag(state);
				inputSystem.Update(delta);
			}
		}
	}

	public Axis? GetAxisObject(string name) => GetSystemsForState(State)
		.Select(inputSystem => inputSystem.GetAxisObject(name))
		.FirstOrDefault(axisObject => axisObject != null);

	public float Axis(string name) => GetAxisObject(name)?.Value ?? 0;

	public bool Button(string buttonName) => GetAxisObject(buttonName)?.Pressed ?? false;

	public bool JustPressed(string buttonName) => GetAxisObject(buttonName)?.JustPressed ?? false;

	public bool JustReleased(string buttonName) => GetAxisObject(buttonName)?.JustReleased ?? false;
}