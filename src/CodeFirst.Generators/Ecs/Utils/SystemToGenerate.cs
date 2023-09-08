using System.Collections.Immutable;

namespace CodeFirst.Generators.Ecs.Utils;

internal readonly record struct SystemToGenerate(string? Namespace, string SystemName, string MethodName,
	ImmutableArray<ParameterDeclaration> ParameterDeclarations);