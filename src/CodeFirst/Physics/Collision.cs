using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Physics;

[PublicAPI]
public record Collision(Guid Self, Guid Other, CollisionResolution Resolution, Vector2 Normal, float Theta)
{
	public Vector2 Separation { get; } = Normal * Theta;
}