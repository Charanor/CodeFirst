namespace JXS.Graphics.Core;

public enum VertexAttributeLocation
{
	Position = 0,
	Normal = 1,
	Color = 2,
	TexCoords = 3
}

public static class VertexAttributeLocationExtensions
{
	public static string GetConstantName(this VertexAttributeLocation @this)
	{
		var name = Enum.GetName(@this);
		if (name == null)
		{
			throw new NullReferenceException($"Could not find {nameof(VertexAttributeLocation)} name for value {name}");
		}

		return $"VERTEX_{ToCapitalSnakeCase(name)}_IDX";
	}
	
	public static string GetLayoutName(this VertexAttributeLocation @this)
	{
		var name = Enum.GetName(@this);
		if (name == null)
		{
			throw new NullReferenceException($"Could not find {nameof(VertexAttributeLocation)} name for value {name}");
		}

		return $"VERTEX_{ToCapitalSnakeCase(name)}_LAYOUT";
	}

	private static string ToCapitalSnakeCase(string input) => string
		.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToUpper();
}