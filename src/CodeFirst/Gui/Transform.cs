using OpenTK.Mathematics;

namespace CodeFirst.Gui;

public class Transform
{
	public Vector2 Position { get; set; }

	public static Transform operator +(Transform left, Transform right) => new()
	{
		Position = left.Position + right.Position
	};

	public Box2 Apply(Box2 input) => Box2.FromPositions(input.Min + Position, input.Max + Position);
}