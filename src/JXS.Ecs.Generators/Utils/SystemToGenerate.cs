using System.Collections.Immutable;

namespace Ecs.Generators.Utils;

internal readonly record struct SystemToGenerate(string? Namespace, string SystemName, string MethodName,
	ImmutableArray<ParameterDeclaration> ParameterDeclarations);