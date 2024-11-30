using CodeFirst.Utils;

namespace CodeFirst.Ecs.Core;

/// <summary>
///     An <see cref="IteratingSystem" /> that processes all its entities in parallel using a fork-and-join model.
///     Parallelization has a substantial performance overhead so this implementation is only useful when there are a
///     large number of entities or each individual entity takes a long time to process. The number of parallel
///     instances is limited by the running computer's core count.
/// </summary>
public abstract class ParallelIteratingSystem : IteratingSystem
{
	private readonly Dictionary<int, Entity> threadedCurrentEntity = new();

	/// <summary>
	///     The minimum number of entities that each thread should process. Setting this is useful for systems that
	///     have a lot of entities but each individual entity does not process for very long, for example if it takes
	///     1ms for this system to process 1 000 entities and you have 1 000 000 entities it might be a good idea to
	///     set this to <c>1000</c> to limit processing of this system to 1ms (+ some small overhead).
	/// </summary>
	/// <remarks>Defaults to <c>1</c>. Setting this to <c>&lt;= 0</c> does nothing special.</remarks>
	public int MinimumEntitiesPerThread { get; set; } = 1;

	public new Entity CurrentEntity
	{
		get
		{
			if (!Thread.CurrentThread.IsThreadPoolThread)
			{
				DevTools.Throw<ParallelIteratingSystem>(
					new InvalidOperationException($"Cannot get {nameof(CurrentEntity)} from non-ThreadPool thread"));
				return Entity.Invalid;
			}

			return threadedCurrentEntity[Environment.CurrentManagedThreadId];
		}
		set
		{
			if (!Thread.CurrentThread.IsThreadPoolThread)
			{
				DevTools.Throw<ParallelIteratingSystem>(
					new InvalidOperationException($"Cannot set {nameof(CurrentEntity)} from non-ThreadPool thread"));
				return;
			}

			threadedCurrentEntity[Environment.CurrentManagedThreadId] = value;
		}
	}

	public override void Update(float delta)
	{
		if (Entities.Count <= 0)
		{
			return;
		}

		ThreadPool.GetAvailableThreads(out var threadCount, out _);
		var (entities, size) = Entities.Begin();

		var entitiesPerThread = (int)Math.Max(MathF.Ceiling((float)size / threadCount), MinimumEntitiesPerThread);
		var taskCount = (int)Math.Ceiling((float)size / entitiesPerThread);

		using var countdown = new CountdownEvent(taskCount);
		for (var i = 0; i < taskCount; i++)
		{
			ThreadPool.QueueUserWorkItem(ProcessEntities, entitiesPerThread * i, preferLocal: false);
			continue;

			void ProcessEntities(int offset)
			{
				var remainingEntities = Math.Min(entitiesPerThread, entities.Length - offset);
				for (var entityIndex = 0; entityIndex < remainingEntities; entityIndex++)
				{
					var entity = entities[offset + entityIndex];
					Update(entity, delta);
				}

				// ReSharper disable once AccessToDisposedClosure
				countdown.Signal();
			}
		}

		countdown.Wait();

		Entities.Commit();
	}
}