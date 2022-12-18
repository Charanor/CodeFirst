using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecs.Generators.Utils;

internal static class GenerationUtils
{
	private const string GENERATED_FIELD_PREFIX = "____generated_";
	private const string GENERATED_MAPPER_SUFFIX = "_do_not_use_explicitly_use_utility_methods_instead";

	private const string ENTITY_TYPE_NAME = "Entity";
	private const string DELTA_TIME_TYPE_NAME = "float";

	private const string HIDE_ATTRIBUTE = "[EditorBrowsable(EditorBrowsableState.Never)]";

	// Field and parameter names
	private const string DEFAULT_ENTITY_PARAM_NAME = "entity";
	private const string DEFAULT_DELTA_PARAM_NAME = "delta";

	// Namespaces
	private const string COMPONENT_MAPPER_NAMESPACE = "JXS.Ecs.Core";

	private const string REF = "ref";
	private const string IN = "in";
	private const string REF_READONLY = "ref readonly";

	// Fields and property names
	private const string CURRENT_ENTITY = "CurrentEntity";
	private const string NO_ENTITY = "Entity.Invalid";

	// External functions
	private const string ASSERT_HAS_ENTITY = "AssertHasEntity";

	// Utility method names
	private const string REMOVE_METHOD = "Remove";
	private const string CREATE_METHOD = "Create";

	// Docstrings
	private const string COMPONENT_MAPPER_DOC_SUMMARY =
		$"DO NOT ACCESS MANUALLY! Use the utility methods ({REMOVE_METHOD}, etc.) instead! Accessing manually can cause unpredictable behavior!";

	private static readonly IEnumerable<string> BuiltInNamespaces = new[]
	{
		"JXS.Ecs.Core",
		"System.ComponentModel"
	};

	public static string GenerateSystemClasses(IImmutableList<SystemToGenerate> systemsToGenerate)
	{
		var builder = new ClassBuilder();

		var namespaces = systemsToGenerate
			.SelectMany(sys => sys.ParameterDeclarations)
			.Where(param => param.Type is not ENTITY_TYPE_NAME and not DELTA_TIME_TYPE_NAME)
			.Select(param => param.Namespace)
			.Concat(BuiltInNamespaces)
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
			var componentParameters = parameters
				.Where(param => param.Type is not ENTITY_TYPE_NAME and not DELTA_TIME_TYPE_NAME)
				.ToImmutableList();
			var entityParameterName = parameters
				.FirstOrDefault(param => param.Type is ENTITY_TYPE_NAME)
				.NameOrDefaultIfEmpty(DEFAULT_ENTITY_PARAM_NAME);
			var deltaParameterName = parameters
				.FirstOrDefault(param => param.Type is DELTA_TIME_TYPE_NAME)
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
					builder.DocstringBlock(tag: "summary", COMPONENT_MAPPER_DOC_SUMMARY);
					builder.IndentedLn(HIDE_ATTRIBUTE);
					builder.IndentedLn(
						$"private readonly {COMPONENT_MAPPER_NAMESPACE}.ComponentMapper<{parameterDeclaration.Type}> {MapperName(parameterDeclaration.Type)} = null!;"
					);
				}

				// Generate Remove methods
				foreach (var decl in componentParameters.Where(decl => decl.Modifier == REF))
				{
					builder.BeginBlock($"private void {REMOVE_METHOD}({REF} {decl.Type} {decl.Name})");
					{
						builder.FunctionCall(ASSERT_HAS_ENTITY, $"nameof({REMOVE_METHOD})");
						builder.IndentedLn($"{MapperName(decl.Type)}.Remove({CURRENT_ENTITY});");
						// We set it to null here to make sure the user gets a NullReferenceException if they try to access it
						builder.AssignmentOp(decl.Name, value: "default");
					}
					builder.EndBlock();
				}

				// Generate optional component utilities
				foreach (var decl in componentParameters.Where(decl => decl.Optional))
				{
					var generatedFieldName = OptionalComponentFieldName(decl.Type);
					builder.IndentedLn($"{HIDE_ATTRIBUTE}private bool {generatedFieldName};");
					builder.BeginBlock($"private bool Has{decl.Type}");
					{
						builder.BeginBlock("get");
						{
							builder.FunctionCall(ASSERT_HAS_ENTITY, $"nameof(Has{decl.Type})");
							builder.IndentedLn($"return {generatedFieldName};");
						}
						builder.EndBlock();
					}
					builder.EndBlock();

					builder.BeginBlock($"private ref {decl.Type} {CREATE_METHOD}({IN} {decl.Type} {decl.Name})");
					{
						builder.FunctionCall(ASSERT_HAS_ENTITY, $"nameof({CREATE_METHOD})");
						builder.IndentedLn($"{OptionalComponentFieldName(decl.Type)} = true;");
						builder.IndentedLn(
							$"return ref {MapperName(decl.Type)}.AddIfMissing({CURRENT_ENTITY}, {IN} {decl.Name});");
					}
					builder.EndBlock();
				}

				// Generate update method
				builder.BeginBlock(
					$"protected override void Update({ENTITY_TYPE_NAME} {entityParameterName}, {DELTA_TIME_TYPE_NAME} {deltaParameterName})"
				);
				{
					builder.AssignmentOp(CURRENT_ENTITY, entityParameterName);
					foreach (var decl in componentParameters)
					{
						if (!decl.Optional)
						{
							var varModifier = decl.Modifier != REF ? REF_READONLY : REF;
							builder.IndentedLn(
								$"{varModifier} {decl.Type} {decl.Name} = {REF} {MapperName(decl.Type)}.Get(entity);");
						}
						else
						{
							builder.IndentedLn(
								$"{OptionalComponentFieldName(decl.Type)} = {MapperName(decl.Type)}.Has(entity);");
							switch (decl.Modifier)
							{
								case REF:
									builder.IndentedLn(
										$"var startedWith{decl.Type} = {OptionalComponentFieldName(decl.Type)};");
									builder.IndentedLn(
										$"{decl.Type} {decl.Name} = {OptionalComponentFieldName(decl.Type)} ? {MapperName(decl.Type)}.Get(entity) : default;");
									break;
								default:
									builder.IndentedLn(
										$"{REF_READONLY} {decl.Type} {decl.Name} = {REF} {MapperName(decl.Type)}.GetComponentDataFor(entity);");
									break;
							}
						}
					}

					var paramList = parameters.Select(
						param => $"{param.Modifier ?? ""} {param.Name}"
					);
					builder.FunctionCall(system.MethodName, paramList);

					var assignableOptionals = componentParameters.Where(param => param is
					{
						Optional: true,
						Modifier: REF
					});
					foreach (var decl in assignableOptionals)
					{
						builder.BeginBlock($"if ({OptionalComponentFieldName(decl.Type)})");
						{
							builder.IndentedLn($"{MapperName(decl.Type)}.Update(entity, {IN} {decl.Name});");
						}
						builder.EndBlock();
						builder.BeginBlock($"else if (!startedWith{decl.Type} && {decl.Name} != default)");
						{
							builder.IndentedLn(
								$"throw new System.InvalidOperationException({Quote($"Must not assign to optional component parameter {decl.Name} when the component does not exist. Instead, use Has{decl.Type} to check if the component exists, and if it does not, use the {CREATE_METHOD}({IN} {decl.Type}) to create a new instance.")});");
						}
						builder.EndBlock();
					}

					builder.AssignmentOp(CURRENT_ENTITY, NO_ENTITY);
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

	private static void AssignmentOp(this ClassBuilder builder, string variable, IConvertible value)
	{
		builder.IndentedLn($"{variable} = {value};");
	}

	private static void FunctionCall(
		this ClassBuilder builder,
		string funcName,
		params string[] arguments
	) => builder.FunctionCall(funcName, arguments.AsEnumerable());

	private static void FunctionCall(
		this ClassBuilder builder,
		string funcName,
		IEnumerable<string> arguments
	)
	{
		builder.IndentedLn($"{funcName}({string.Join(separator: ", ", arguments)});");
	}

	private static string Quote(string str) => $@"""{str}""";

	private static string MapperName(string componentName) =>
		$"{GENERATED_FIELD_PREFIX}{componentName}_mapper{GENERATED_MAPPER_SUFFIX}";

	private static string OptionalComponentFieldName(string componentType) =>
		$"{GENERATED_FIELD_PREFIX}entity_has_{componentType}";
}