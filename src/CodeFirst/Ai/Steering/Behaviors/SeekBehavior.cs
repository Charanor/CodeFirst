using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Ai.Steering.Behaviors;

[PublicAPI]
public class SeekBehavior<TSteerable> : ISteeringBehavior<TSteerable> where TSteerable : ISteerable
{
	public SeekBehavior(Vector3 target)
	{
		Target = target;
	}

	public Vector3 Target { get; protected set; }

	public virtual SteeringDelta Calculate(in TSteerable steerable)
	{
		var direction = Target - steerable.Position;
		var desiredVelocity = direction.LengthSquared < float.Epsilon
			? Vector3.Zero // We have reached the target, stop!
			: Vector3.Normalize(direction) * steerable.MaxLinearVelocity;

		var acceleration = desiredVelocity - steerable.LinearVelocity;
		// ReSharper disable once InvertIf
		if (acceleration.Length > steerable.MaxLinearAcceleration)
		{
			acceleration.Normalize();
			acceleration *= steerable.MaxLinearAcceleration;
		}

		return acceleration;
	}
}