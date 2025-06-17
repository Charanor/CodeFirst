using OpenTK.Mathematics;

namespace CodeFirst.Graphics.Core;

public abstract class Material : IDisposable
{
	private const int NULL_UNIFORM = -1;

	private int modelLoc = NULL_UNIFORM;
	private int viewLoc = NULL_UNIFORM;
	private int projLoc = NULL_UNIFORM;

	public abstract ShaderProgram Shader { get; }

	public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
	public Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;
	public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Shader.Dispose();
	}

	protected abstract void ApplyExtended();

	public void Apply()
	{
		// Update uniform locations
		if (modelLoc == NULL_UNIFORM)
		{
			modelLoc = Shader.GetUniformLocation("modelMatrix");
		}

		if (viewLoc == NULL_UNIFORM)
		{
			viewLoc = Shader.GetUniformLocation("viewMatrix");
		}

		if (projLoc == NULL_UNIFORM)
		{
			projLoc = Shader.GetUniformLocation("projectionMatrix");
		}

		// Apply uniform values
		if (modelLoc != NULL_UNIFORM)
		{
			Shader.SetUniform(modelLoc, ModelMatrix);
		}

		if (viewLoc != NULL_UNIFORM)
		{
			Shader.SetUniform(viewLoc, ViewMatrix);
		}

		if (projLoc != NULL_UNIFORM)
		{
			Shader.SetUniform(projLoc, ProjectionMatrix);
		}

		ApplyExtended();
	}
}