namespace CodeFirst.Utils;

/// <summary>
///		Used to count FPS in the main game loop. Instantiate a new instance and attach a handler to the <see cref="OnFps"/>
///		event, and make sure to call <see cref="Update"/> and <see cref="Draw"/> methods in the respective update and
///		draw frames.
/// </summary>
/// <remarks>
///		If <see cref="Update"/> or <see cref="Draw"/> is not called FPS for that type will simply not be recorded.
/// </remarks>
public class FpsCounter
{
	private const double ONE_SECOND = 1f;

	private int updateCount;
	private double updateTimer;

	private int drawCount;
	private double drawTimer;

	public void Update(double delta)
	{
		updateCount += 1;
		updateTimer += delta;

		if (updateTimer < ONE_SECOND)
		{
			return;
		}

		OnFps?.Invoke(this, new FpsEvent(FpsType.Update, updateCount / updateTimer));
		updateCount = 0;
		updateTimer = 0;
	}

	public void Draw(double delta)
	{
		drawCount += 1;
		drawTimer += delta;

		if (drawTimer < ONE_SECOND)
		{
			return;
		}

		OnFps?.Invoke(this, new FpsEvent(FpsType.Draw, drawCount / drawTimer));
		drawCount = 0;
		drawTimer = 0;
	}

	public event EventHandler<FpsEvent>? OnFps;
}

public readonly record struct FpsEvent(FpsType Type, double Fps);

public enum FpsType
{
	Update,
	Draw
}