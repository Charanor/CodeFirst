using OpenTK.Mathematics;

namespace CodeFirst.Physics;

public record CollisionResult(Vector2 Position, List<Collision> Collisions)
{
	public static CollisionResult Default => new(Vector2.Zero, []);
}