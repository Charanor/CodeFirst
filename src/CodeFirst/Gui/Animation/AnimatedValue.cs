using CodeFirst.Utils.Logging;

namespace CodeFirst.Gui.Animation;

public record AnimatedValue(float TargetValue)
{
	private static readonly ILogger Logger = LoggingManager.Get<AnimatedValue>();

	private float accumulatedTime;

	protected float StartValue { get; private set; }

	public bool Initialized { get; private set; }
	public float Value { get; private set; }

	protected virtual float CalculateValueAtTime(float timeSinceStart) => TargetValue;

	public void Initialize(float initialStartValue)
	{
		if (Initialized)
		{
			const string error = "AnimatedValue has already been initialized!";
			Logger.Error(error);
#if DEBUG
			throw new InvalidOperationException(error);
#else
				return;
#endif
		}

		StartValue = initialStartValue;
		Initialized = true;
	}

	public float Update(float delta)
	{
		if (!Initialized)
		{
			const string error = "AnimatedValue must be initialized before use!";
			Logger.Error(error);
#if DEBUG
			throw new InvalidOperationException(error);
#else
				return;
#endif
		}

		accumulatedTime += delta;
		Value = CalculateValueAtTime(accumulatedTime);
		return Value;
	}

	public static implicit operator float(AnimatedValue value) => value.Value;
	public static implicit operator AnimatedValue(float value) => new(value);
}