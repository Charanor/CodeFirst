using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeFirst.Generators.Ecs.Utils;

internal static class GenerationUtils
{
	private const string GENERATED_FIELD_PREFIX = "____generated_";
	private const string GENERATED_MAPPER_SUFFIX = "_do_not_use_explicitly_use_utility_methods_instead";

	public static void AssignmentOp(this ClassBuilder builder, string variable, IConvertible value)
	{
		builder.IndentedLn($"{variable} = {value};");
	}

	public static void FunctionCall(
		this ClassBuilder builder,
		string funcName,
		params string[] arguments
	) => builder.FunctionCall(funcName, arguments.AsEnumerable());

	public static void FunctionCall(
		this ClassBuilder builder,
		string funcName,
		IEnumerable<string> arguments
	)
	{
		builder.IndentedLn($"{funcName}({string.Join(", ", arguments)});");
	}

	public static void FunctionCall(
		this ClassBuilder builder,
		string funcName,
		string firstArgument,
		IEnumerable<string> arguments
	)
	{
		builder.IndentedLn($"{funcName}({firstArgument}, {string.Join(", ", arguments)});");
	}

	public static ComponentMapperContext ComponentMapper(this ClassBuilder builder, string componentName,
		string? componentNamespace, bool isSingleton, bool isIteratingSystem)
	{
		var ns = componentNamespace?.Replace(".", "_") ?? "";
		var fieldName = $"{GENERATED_FIELD_PREFIX}{ns}_{componentName}_mapper{GENERATED_MAPPER_SUFFIX}";
		var componentTypename = componentNamespace == null ? componentName : $"{componentNamespace}.{componentName}";

		builder.IndentedLn("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
		builder.IndentedLn(
			isSingleton
				? $"private readonly {SingletonComponentMapperType}<{componentTypename}> {fieldName} = null!;"
				: $"private readonly {ComponentMapperType}<{componentTypename}> {fieldName} = null!;");

		return new ComponentMapperContext(fieldName, componentName, componentNamespace, isSingleton, isIteratingSystem);
	}

	public static void UtilityMethods(this ClassBuilder builder, ComponentMapperContext context,
		bool isReadOnly = false, bool generateCurrentEntityVariants = true)
	{
		if (context.IsSingleton)
		{
			SingletonField(builder, context, isReadOnly);
			return;
		}

		HasComponentUtilityMethod(builder, context, generateCurrentEntityVariants);
		GetComponentUtilityMethod(builder, context, isReadOnly, generateCurrentEntityVariants);
		if (!isReadOnly)
		{
			CreateComponentUtilityMethod(builder, context, generateCurrentEntityVariants);
		}
	}

	private static void SingletonField(ClassBuilder builder, ComponentMapperContext context, bool isReadOnly)
	{
		var (fieldName, name, ns, _, _) = context;
		var componentTypename = ns == null ? name : $"{ns}.{name}";
		builder.IndentedLn(
			$"private {(isReadOnly ? "ref readonly" : "ref")} {componentTypename} {name} => ref {fieldName}.SingletonInstance;");
	}

	private static void GetComponentUtilityMethod(ClassBuilder builder, ComponentMapperContext context, bool isReadOnly,
		bool generateCurrentEntityVariants)
	{
		var (fieldName, name, ns, _, isIteratingSystem) = context;
		var componentTypename = ns == null ? name : $"{ns}.{name}";
		builder.IndentedLn(
			$"private {(isReadOnly ? "ref readonly" : "ref")} {componentTypename} Get{name}ForEntity({EntityType} entity) => ref {fieldName}.Get(entity);");
		if (isIteratingSystem && generateCurrentEntityVariants)
		{
			builder.IndentedLn(
				$"private {(isReadOnly ? "ref readonly" : "ref")} {componentTypename} {name} => ref Get{name}ForEntity(CurrentEntity);");
		}
	}

	private static void HasComponentUtilityMethod(ClassBuilder builder, ComponentMapperContext context,
		bool generateCurrentEntityVariants)
	{
		var (fieldName, name, _, _, isIteratingSystem) = context;
		builder.IndentedLn($"private bool EntityHas{name}({EntityType} entity) => {fieldName}.Has(entity);");
		if (isIteratingSystem && generateCurrentEntityVariants)
		{
			builder.IndentedLn($"private bool Has{name} => EntityHas{name}(CurrentEntity);");
		}
	}

	private static void CreateComponentUtilityMethod(ClassBuilder builder, ComponentMapperContext context,
		bool generateCurrentEntityVariants)
	{
		var (fieldName, name, ns, _, isIteratingSystem) = context;
		var componentTypename = ns == null ? name : $"{ns}.{name}";
		builder.IndentedLn(
			$"private ref {componentTypename} Create{name}ForEntity({EntityType} entity, in {componentTypename} component) => ref {fieldName}.AddIfMissing(entity, in component);");

		if (isIteratingSystem && generateCurrentEntityVariants)
		{
			builder.IndentedLn(
				$"private ref {componentTypename} Create{name}(in {componentTypename} component) => ref Create{name}ForEntity(CurrentEntity, in component);");
		}
	}

	#region Typenames

	// ReSharper disable InconsistentNaming

	private const string ComponentMapperType = "CodeFirst.Ecs.Core.ComponentMapper";
	private const string SingletonComponentMapperType = "CodeFirst.Ecs.Core.SingletonComponentMapper";
	private const string EntityType = "CodeFirst.Ecs.Core.Entity";

	// ReSharper restore InconsistentNaming

	#endregion
}