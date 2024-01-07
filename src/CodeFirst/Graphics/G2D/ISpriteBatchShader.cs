using CodeFirst.Graphics.Core;
using OpenTK.Mathematics;

namespace CodeFirst.Graphics.G2D;

public interface ISpriteBatchShader
{
	Texture2D? Texture0 { get; set; }
	Matrix4 ProjectionMatrix { get; set; }
	Matrix4 ViewMatrix { get; set; }

	void Bind();
	void Unbind();
}