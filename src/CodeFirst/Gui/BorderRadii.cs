namespace CodeFirst.Gui;

public record BorderRadii
{
	public static readonly BorderRadii Zero = new(topLeft: 0, topRight: 0, bottomLeft: 0, bottomRight: 0);

	public BorderRadii()
	{
	}

	public BorderRadii(float topLeft, float topRight, float bottomLeft, float bottomRight)
	{
		TopLeft = topLeft;
		TopRight = topRight;
		BottomLeft = bottomLeft;
		BottomRight = bottomRight;
	}

	public float TopLeft { get; init; }
	public float TopRight { get; init; }
	public float BottomRight { get; init; }
	public float BottomLeft { get; init; }

	public void Deconstruct(out float topLeft, out float topRight, out float bottomLeft, out float bottomRight)
	{
		topLeft = TopLeft;
		topRight = TopRight;
		bottomLeft = BottomLeft;
		bottomRight = BottomRight;
	}
}