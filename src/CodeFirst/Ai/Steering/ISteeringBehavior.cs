using JetBrains.Annotations;

namespace CodeFirst.Ai.Steering;

[PublicAPI]
public interface ISteeringBehavior<TSteerable>  where TSteerable : ISteerable
{
	SteeringDelta Calculate(in TSteerable steerable);
}