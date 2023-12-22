namespace CodeFirst.Generators.Ecs.Utils;

public record ComponentMapperContext(string FieldName, string ComponentName, string? ComponentNamespace,
	bool IsSingleton,
	bool IsIteratingSystem);