using OpenTK.Windowing.GraphicsLibraryFramework;

namespace JXS.Input.Core;

public record KeyboardAxis : Axis
{
	private readonly Keys negative;
	private readonly Keys positive;

	public KeyboardAxis(Keys positive, Keys negative)
	{
		this.positive = positive;
		this.negative = negative;
	}

	public override float Value
	{
		get
		{
			var posPressed = KeyboardState.IsKeyDown(positive);
			var negPressed = KeyboardState.IsKeyDown(negative);
			if (posPressed == negPressed)
			{
				return 0;
			}

			if (posPressed)
			{
				return 1;
			}

			return -1;
		}
	}
}