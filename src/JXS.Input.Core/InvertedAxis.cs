namespace JXS.Input.Core;

public record InvertedAxis(Axis Axis) : Axis
{
	public override float Value => Axis.Value != 0 ? 0 : 1;

	public override void Update(float gameTime)
	{
		base.Update(gameTime);
		Axis.Update(gameTime);
	}
}