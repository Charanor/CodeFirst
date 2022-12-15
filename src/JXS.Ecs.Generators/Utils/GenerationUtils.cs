using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ecs.Generators.Utils;

internal static class GenerationUtils
{
    private const string GENERATED_FIELD_PREFIX = "____";
    
    private const string ENTITY_TYPE_NAME = "Entity";
    private const string DELTA_TIME_TYPE_NAME = "float";

    // Field and parameter names
    private const string DEFAULT_ENTITY_PARAM_NAME = "entity";
    private const string DEFAULT_DELTA_PARAM_NAME = "delta";

    // Namespaces
    private const string COMPONENT_MAPPER_NAMESPACE = "JXS.Ecs.Core";

    private const string REF = "ref";

    // Fields and property names
    private const string CURRENT_ENTITY = "CurrentEntity";
    private const string NO_ENTITY = "Entity.Invalid";

    // External functions
    private const string ASSERT_HAS_ENTITY = "AssertHasEntity";

    // Utility method names
    private const string REMOVE_METHOD = "Remove";

    // Docstrings
    private const string COMPONENT_MAPPER_DOC_SUMMARY =
        $"DO NOT ACCESS MANUALLY! Use the utility methods ({REMOVE_METHOD}, etc.) instead! Accessing manually can cause unpredictable behavior!";

    private static readonly IEnumerable<string> BuiltInNamespaces = new[]
    {
        "JXS.Ecs.Core"
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
                    builder.IndentedLn(
                        $"private readonly {COMPONENT_MAPPER_NAMESPACE}.ComponentMapper<{parameterDeclaration.Type}> {MapperName(parameterDeclaration.Name)} = null!;"
                    );
                }

                // Generate utility methods
                foreach (
                    var parameterDeclaration in componentParameters.Where(
                        parameterDeclaration => parameterDeclaration.Modifier == REF
                    )
                )
                {
                    builder.BeginBlock(
                        $"private void Remove({REF} {parameterDeclaration.Type} {parameterDeclaration.Name})"
                    );
                    {
                        builder.FunctionCall(ASSERT_HAS_ENTITY, $"nameof({REMOVE_METHOD})");
                        builder.IndentedLn(
                            $"{MapperName(parameterDeclaration.Name)}.Remove({CURRENT_ENTITY});"
                        );
                        // We set it to null here to make sure the user gets a NullReferenceException if they try to access it
                        builder.AssignmentOp(parameterDeclaration.Name, value: "null");
                    }
                    builder.EndBlock();
                }

                // Generate update method
                builder.BeginBlock(
                    $"protected override void Update({ENTITY_TYPE_NAME} {entityParameterName}, {DELTA_TIME_TYPE_NAME} {deltaParameterName})"
                );
                {
                    builder.AssignmentOp(CURRENT_ENTITY, entityParameterName);
                    foreach (var parameterDeclaration in componentParameters)
                    {
                        if (!parameterDeclaration.Optional)
                        {
                            builder.IndentedLn(
                                $"{REF} {parameterDeclaration.Type} {parameterDeclaration.Name} = {REF} {MapperName(parameterDeclaration.Name)}.Get(entity);"
                            );
                        }
                        else
                        {
                            builder.IndentedLn(
                                $"var has{parameterDeclaration.Type} = {MapperName(parameterDeclaration.Name)}.Has(entity);"
                            );
                            builder.IndentedLn(
                                $"{parameterDeclaration.Type}? {parameterDeclaration.Name} = has{parameterDeclaration.Type} ? {MapperName(parameterDeclaration.Name)}.Get(entity) : null;"
                            );
                        }
                    }

                    var paramList = parameters.Select(
                        param => $"{param.Modifier ?? ""} {param.Name}"
                    );
                    builder.FunctionCall(system.MethodName, paramList);

                    foreach (var parameterDeclaration in componentParameters)
                    {
                        if (!parameterDeclaration.Optional || parameterDeclaration.Modifier != REF)
                        {
                            continue;
                        }

                        builder.BeginBlock($"if (has{parameterDeclaration.Type})");
                        {
                            //builder.BeginBlock($"if (!{parameterDeclaration.Name}.HasValue)");
                            builder.BeginBlock($"if ({parameterDeclaration.Name} == null)");
                            {
                                builder.IndentedLn(
                                    $"throw new System.NullReferenceException(nameof({parameterDeclaration.Name}));"
                                );
                            }
                            builder.EndBlock();
                            builder.BeginBlock("else");
                            {
                                // 	builder.IndentedLn(
                                // 		$"{MapperName(parameterDeclaration.Name)}.Update(entity, {parameterDeclaration.Name}.Value);");
                                builder.IndentedLn(
                                    $"{MapperName(parameterDeclaration.Name)}.Update(entity, in {parameterDeclaration.Name});"
                                );
                            }
                            builder.EndBlock();
                        }
                        builder.EndBlock();
                        //builder.BeginBlock($"else if ({parameterDeclaration.Name}.HasValue)");
                        builder.BeginBlock($"else if ({parameterDeclaration.Name} != null)");
                        {
                            // builder.IndentedLn(
                            // 	$"{MapperName(parameterDeclaration.Name)}.Add(entity, {parameterDeclaration.Name}.Value);");
                            builder.IndentedLn(
                                $"{MapperName(parameterDeclaration.Name)}.Add(entity, in {parameterDeclaration.Name});"
                            );
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

    private static void AssertHasEntity(this ClassBuilder builder, string methodName)
    {
        builder.IndentedLn($"{ASSERT_HAS_ENTITY}(nameof({methodName}));");
    }

    private static string Quote(string str) => $@"""{str}""";

    private static string MapperName(string componentName) => $"{GENERATED_FIELD_PREFIX}{componentName}Mapper";
}
