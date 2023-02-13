namespace JXS.Utils.Logging;

public class IndentHandle : IDisposable
{
	private readonly ILogger logger;
	private bool disposed;

	public IndentHandle(ILogger logger)
	{
		this.logger = logger;
		logger.Indent();
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}

		GC.SuppressFinalize(this);
		logger.Dedent();
		disposed = true;
	}

	public void Dedent()
	{
		Dispose();
	}
}