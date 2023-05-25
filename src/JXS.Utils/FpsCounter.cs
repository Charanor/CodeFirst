namespace JXS.Utils;

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