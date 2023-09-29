using CodeFirst.Utils.Events;

namespace CodeFirst.Ai.State;

public abstract class StateMachine<TState> where TState : Enum
{
	private TState state;

	protected StateMachine(TState state)
	{
		this.state = state;
	}

	public TState State
	{
		get => state;
		protected set
		{
			if (EqualityComparer<TState>.Default.Equals(state, value))
			{
				return;
			}

			var oldState = state;
			OnStateExit(value);
			state = value;
			OnStateEnter(oldState);
			OnStateChanged?.Invoke(this, new StateChangedArgs<TState>(oldState, state));
		}
	}

	public event EventHandler<StateMachine<TState>, StateChangedArgs<TState>>? OnStateChanged;

	/// <summary>
	///     Called once per frame when this state machine is being updated. Access the current state through
	///     the <see cref="State" /> property. Change state by setting the <see cref="State" /> property.
	/// </summary>
	/// <param name="delta"></param>
	public abstract void Process(float delta);

	/// <summary>
	///     Called when a new state has been entered. Called right after <see cref="OnStateExit" /> was called
	///     for the previous state. During this call the <see cref="State" /> property references the just entered state.
	/// </summary>
	/// <param name="previousState">the previous state that we just left</param>
	protected abstract void OnStateEnter(TState previousState);

	/// <summary>
	///     Called when the current state (accessed via the <see cref="State" /> property) is about to be changed.
	///     This is called right before <see cref="OnStateEnter" /> is called.
	/// </summary>
	/// <param name="upcomingState">the state that will become the new state</param>
	protected abstract void OnStateExit(TState upcomingState);
}

public class StateChangedArgs<TState> : EventArgs where TState : Enum
{
	public StateChangedArgs(TState previousState, TState newState)
	{
		PreviousState = previousState;
		NewState = newState;
	}

	public TState PreviousState { get; }
	public TState NewState { get; }
}