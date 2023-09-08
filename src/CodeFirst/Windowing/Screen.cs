namespace CodeFirst.Windowing;

public abstract class Screen
{
	public Game? Game { get; internal set; }

	public virtual void Show()
	{
	}

	public virtual void Update(float delta)
	{
	}

	public virtual void Draw(float delta)
	{
	}

	public virtual void Hide()
	{
	}

	public virtual void Dispose()
	{
	}

	public virtual void Resized(int width, int height)
	{
	}
}