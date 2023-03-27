using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace JXS.Ai.Steering;

[PublicAPI]
public interface ISteerable
{
	Vector3 Position { get; }
	Vector3 LinearVelocity { get; }
	float MaxLinearVelocity { get; }
	float MaxLinearAcceleration { get; }

	float Angle { get; }
	float AngularVelocity { get; }
	float MaxAngularVelocity { get; }
	float MaxAngularAcceleration { get; }
}