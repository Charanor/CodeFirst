using CodeFirst.Graphics.Core;
using CodeFirst.Utils;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public class SpriteBatch : IBatch
{
	private bool isBlendingEnabled;

	public bool IsBlendingEnabled
	{
		get => isBlendingEnabled;
		set => isBlendingEnabled = value;
	}

	public BlendingFactor SourceBlendFactor { get; set; }
	public BlendingFactor DestinationBlendFactor { get; set; }
	
	public Matrix4 ProjectionMatrix { get; set; }
	public bool IsBatching { get; private set; }

	public void Begin()
	{
		if (IsBatching)
		{
			DevTools.Throw<SpriteBatch>(new InvalidOperationException(
				$"Must not call {nameof(SpriteBatch)}#{nameof(Begin)} twice in a row without calling {nameof(End)} in between."));
			return;
		}

		IsBatching = true;
	}

	public void End()
	{
		if (!IsBatching)
		{
			DevTools.Throw<SpriteBatch>(new InvalidOperationException(
				$"Must not call {nameof(SpriteBatch)}#{nameof(End)} without first calling {nameof(Begin)}."));
			return;
		}

		IsBatching = false;
	}

	public void Rasterize()
	{
		if (!IsBatching)
		{
			DevTools.Throw<SpriteBatch>(new InvalidOperationException(
				$"Must not call {nameof(SpriteBatch)}#{nameof(Rasterize)} outside of {nameof(Begin)} & {nameof(End)} block."));
			return;
		}
		
		// TODO rasterize;
		IsBatching = true;
	}

	public void Draw(Texture2D texture, float x, float y, float width = default, float height = default,
		int sourceX = default,
		int sourceY = default, int sourceWidth = default, int sourceHeight = default)
	{
	}

	public void Dispose()
	{
	}
}