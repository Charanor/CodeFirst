using JetBrains.Annotations;

namespace JXS.Ai.Steering.Behaviors;

[PublicAPI]
public class AggregateBehavior<TSteerable> : ISteeringBehavior<TSteerable> where TSteerable : ISteerable
{
	private readonly List<ISteeringBehavior<TSteerable>> steeringBehaviors;

	public AggregateBehavior()
	{
		steeringBehaviors = new List<ISteeringBehavior<TSteerable>>();
	}

	public SteeringDelta Calculate(in TSteerable steerable)
	{
		var result = new SteeringDelta();
		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach (var steeringBehavior in steeringBehaviors)
		{
			result += steeringBehavior.Calculate(steerable);
		}

		return result;
	}

	public void Add(ISteeringBehavior<TSteerable> item) => steeringBehaviors.Add(item);

	public bool Remove(ISteeringBehavior<TSteerable> item) => steeringBehaviors.Remove(item);
}