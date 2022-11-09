namespace JXS.Input.Core;

public record CopyNamedAxis(string AxisName) : Axis
{
	private float value;

	public override float Value => value;

	public override void Update(float gameTime)
	{
		base.Update(gameTime);
		value = InputManager.Instance.Axis(AxisName);
	}
}