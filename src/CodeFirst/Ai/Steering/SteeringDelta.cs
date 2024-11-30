using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Ai.Steering;

[PublicAPI]
public readonly record struct SteeringDelta(Vector3 LinearVelocity, float AngularVelocity)
{
	public static readonly SteeringDelta Zero = default;

	public static implicit operator SteeringDelta(Vector3 linearVelocity) => new(linearVelocity, AngularVelocity: 0);
	public static implicit operator SteeringDelta(float angularVelocity) => new(Vector3.Zero, angularVelocity);

	public static SteeringDelta operator +(SteeringDelta delta, SteeringDelta d2) =>
		new(
			delta.LinearVelocity + d2.LinearVelocity,
			delta.AngularVelocity + d2.AngularVelocity
		);

	public static SteeringDelta operator -(SteeringDelta delta, SteeringDelta d2) =>
		new(
			delta.LinearVelocity - d2.LinearVelocity,
			delta.AngularVelocity - d2.AngularVelocity
		);

	public static SteeringDelta operator +(SteeringDelta delta, Vector3 linearVelocity) =>
		delta with { LinearVelocity = delta.LinearVelocity + linearVelocity };

	public static SteeringDelta operator -(SteeringDelta delta, Vector3 linearVelocity) =>
		delta with { LinearVelocity = delta.LinearVelocity - linearVelocity };

	public static SteeringDelta operator +(SteeringDelta delta, float angularVelocity) =>
		delta with { AngularVelocity = delta.AngularVelocity + angularVelocity };

	public static SteeringDelta operator -(SteeringDelta delta, float angularVelocity) =>
		delta with { AngularVelocity = delta.AngularVelocity - angularVelocity };

	public static SteeringDelta operator *(SteeringDelta delta, float scalar) =>
		delta with { LinearVelocity = delta.LinearVelocity * scalar, AngularVelocity = delta.AngularVelocity * scalar };

	public static SteeringDelta operator /(SteeringDelta delta, float scalar) =>
		delta with { LinearVelocity = delta.LinearVelocity / scalar, AngularVelocity = delta.AngularVelocity / scalar };
}