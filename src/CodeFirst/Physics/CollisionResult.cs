using OpenTK.Mathematics;

namespace CodeFirst.Physics;

public record CollisionResult(Vector2 Position, List<Collision> Collisions)
{
	public static readonly CollisionResult Default = new(Vector2.Zero, new List<Collision>());
}