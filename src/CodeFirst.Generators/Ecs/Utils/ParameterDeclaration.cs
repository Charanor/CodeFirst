namespace CodeFirst.Generators.Ecs.Utils;

internal readonly record struct ParameterDeclaration(string? Modifier, string Type, string Name, string? Namespace,
	bool Optional)
{
	public string NameOrDefaultIfEmpty(string defaultName) => Name is { Length: > 0 } ? Name : defaultName;
}