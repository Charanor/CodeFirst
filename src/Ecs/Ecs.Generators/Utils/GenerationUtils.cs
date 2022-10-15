using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecs.Generators.Utils;

internal static class GenerationUtils
{
	private const string DEFAULT_ENTITY_PARAM_NAME = "entity";
	private const string DEFAULT_DELTA_PARAM_NAME = "delta";

	public static string GenerateSystemClasses(IImmutableList<SystemToGenerate> systemsToGenerate)
	{
		var builder = new ClassBuilder();

		var namespaces = systemsToGenerate
			.SelectMany(sys => sys.ParameterDeclarations)
			.Where(param => param.Type is not "int" && param.Type is not "float")
			.Select(param => param.Namespace)
			.Distinct();

		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach (var ns in namespaces)
		{
			// Generate "using"
			if (ns is null)
			{
				continue;
			}

			builder.IndentedLn($"using {ns};");
		}

		foreach (var system in systemsToGenerate)
		{
			var parameters = system.ParameterDeclarations.ToImmutableList();
			var componentParameters =
				parameters.Where(param => param.Type is not "int" && param.Type is not "float").ToImmutableList();
			var entityParameterName = parameters.FirstOrDefault(param => param.Type is "int")
				.NameOrDefaultIfEmpty(DEFAULT_ENTITY_PARAM_NAME);
			var deltaParameterName = parameters.FirstOrDefault(param => param.Type is "float")
				.NameOrDefaultIfEmpty(DEFAULT_DELTA_PARAM_NAME);

			if (system.Namespace is not null)
			{
				builder.BeginBlock($"namespace {system.Namespace}");
			}

			builder.BeginBlock($"public partial class {system.SystemName}");
			{
				// Generate component mapper definitions
				foreach (var parameterDeclaration in componentParameters)
				{
					builder.IndentedLn(
						$"private readonly MonoGameEngine.Ecs.ComponentMapper<{parameterDeclaration.Type}> {parameterDeclaration.Name}Mapper = null!;");
				}

				builder.BeginBlock($"protected override void Update(int {entityParameterName}, float {deltaParameterName})");
				{
					foreach (var parameterDeclaration in componentParameters)
					{
						if (!parameterDeclaration.Optional)
						{
							builder.IndentedLn(
								$"ref {parameterDeclaration.Type} {parameterDeclaration.Name} = ref {parameterDeclaration.Name}Mapper.Get(entity);");
						}
						else
						{
							builder.IndentedLn(
								$"var has{parameterDeclaration.Type} = {parameterDeclaration.Name}Mapper.Has(entity);");
							builder.IndentedLn(
								$"{parameterDeclaration.Type}? {parameterDeclaration.Name} = has{parameterDeclaration.Type} ? {parameterDeclaration.Name}Mapper.Get(entity) : null;");
						}
					}

					var paramList = parameters.Select(param => $"{param.Modifier ?? ""} {param.Name}");
					builder.IndentedLn($"{system.MethodName}({string.Join(", ", paramList)});");

					foreach (var parameterDeclaration in componentParameters)
					{
						if (!parameterDeclaration.Optional || parameterDeclaration.Modifier != "ref")
						{
							continue;
						}

						builder.BeginBlock($"if (has{parameterDeclaration.Type})");
						{
							builder.BeginBlock($"if (!{parameterDeclaration.Name}.HasValue)");
							{
								//builder.IndentedLn(
								//	$"throw new System.NullReferenceException(nameof({parameterDeclaration.Name}));");
								builder.IndentedLn(
									$"{parameterDeclaration.Name}Mapper.Remove(entity);");
							}
							builder.EndBlock();
							builder.BeginBlock("else");
							{
								builder.IndentedLn(
									$"{parameterDeclaration.Name}Mapper.Update(entity, {parameterDeclaration.Name}.Value);");
							}
							builder.EndBlock();
						}
						builder.EndBlock();
						builder.BeginBlock($"else if ({parameterDeclaration.Name}.HasValue)");
						{
							builder.IndentedLn(
								$"{parameterDeclaration.Name}Mapper.Add(entity, {parameterDeclaration.Name}.Value);");
						}
						builder.EndBlock();
					}
				}
				builder.EndBlock();
			}
			builder.EndBlock();

			if (system.Namespace is not null)
			{
				builder.EndBlock();
			}

			builder.IndentedLn("");
		}

		return builder.Generate();
	}

	public readonly struct SystemToGenerate
	{
		public SystemToGenerate(string? ns, string systemName, string methodName,
			IEnumerable<ParameterDeclaration> parameterDeclarations)
		{
			Namespace = ns;
			SystemName = systemName;
			MethodName = methodName;
			ParameterDeclarations = parameterDeclarations;
		}

		public string? Namespace { get; }
		public string SystemName { get; }
		public string MethodName { get; }
		public IEnumerable<ParameterDeclaration> ParameterDeclarations { get; }
	}

	public readonly struct ParameterDeclaration
	{
		public ParameterDeclaration(string? modifier, string type, string name, string? ns, bool optional)
		{
			Modifier = modifier;
			Type = type;
			Name = name;
			Namespace = ns;
			Optional = optional;
		}

		public string? Modifier { get; }
		public string Type { get; }
		public string Name { get; }
		public string? Namespace { get; }
		public bool Optional { get; }

		public string NameOrDefaultIfEmpty(string defaultName) => Name.Length == 0 ? defaultName : Name;
	}
}