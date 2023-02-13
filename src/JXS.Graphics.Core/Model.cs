namespace JXS.Graphics.Core;

public class Model
{
	public Model(IEnumerable<Mesh> meshes)
	{
		Meshes = meshes.ToList();
	}

	public IReadOnlyList<Mesh> Meshes { get; }
}