using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public interface IDrawable
{
	void Draw(SpriteBatch batch, Box2 region, Color4<Rgba> color = default);
}