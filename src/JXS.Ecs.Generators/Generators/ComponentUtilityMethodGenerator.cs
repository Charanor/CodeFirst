using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Ecs.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

[Generator]
public class ComponentUtilityMethodGenerator : IIncrementalGenerator
{
	private const string ATTRIBUTE_NAMESPACE = "JXS.Ecs.Core.Attributes.Generation";
	private const string ATTRIBUTE_SHORT_NAME = "GenerateComponentUtilities";
	private const string ATTRIBUTE_NAME = $"{ATTRIBUTE_SHORT_NAME}Attribute";
	private const string ATTRIBUTE = $"{ATTRIBUTE_NAMESPACE}.{ATTRIBUTE_NAME}";

	private const string COMPONENT_METADATA_NAME = "JXS.Ecs.Core.IComponent";
	private const string SINGLETON_COMPONENT_METADATA_NAME = "JXS.Ecs.Core.ISingletonComponent";
	private const string ITERATING_SYSTEM_METADATA_NAME = "JXS.Ecs.Core.IteratingSystem";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var declarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, _) => HasAttribute(syntaxNode),
				transform: static (ctx, _) => TransformSyntax(ctx)
			)
			.Where(static syntax => syntax is not null);

		var compilationSystems = context.CompilationProvider.Combine(declarations.Collect());
		context.RegisterSourceOutput(
			compilationSystems,
			action: static (sourceProductionContext, source) =>
				Execute(
					source.Left,
					source.Right.CastArray<IntermediateSyntax>(),
					sourceProductionContext
				)
		);
	}

	private static bool HasAttribute(SyntaxNode node)
	{
		if (node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } syntax)
		{
			return false;
		}

		return syntax.AttributeLists
			.SelectMany(syntaxAttributeList => syntaxAttributeList.Attributes)
			.Any(attr => attr.Name.ToString().Contains(ATTRIBUTE_SHORT_NAME));
	}

	private static IntermediateSyntax TransformSyntax(GeneratorSyntaxContext context)
	{
		var syntax = (ClassDeclarationSyntax)context.Node;

		var semanticModel = context.SemanticModel;
		var compilation = semanticModel.Compilation;
		var componentType = compilation.GetTypeByMetadataName(COMPONENT_METADATA_NAME);
		var singletonComponentType = compilation.GetTypeByMetadataName(SINGLETON_COMPONENT_METADATA_NAME);
		var attributeType = compilation.GetTypeByMetadataName(ATTRIBUTE);

		var allTypes = syntax.AttributeLists
			.SelectMany(list => list.Attributes)
			.Where(IsUtilityAttributeSyntax)
			.Select(attributeSyntax => attributeSyntax.ArgumentList)
			.SelectMany(argumentList => argumentList!.Arguments)
			.Select(argument => argument.Expression)
			.Where(expression => expression is TypeOfExpressionSyntax)
			.Cast<TypeOfExpressionSyntax>()
			.Select(typeofExpression => typeofExpression.Type)
			.Select(typeSyntax => semanticModel.GetTypeInfo(typeSyntax).Type)
			.Distinct(SymbolEqualityComparer.Default)
			.OfType<INamedTypeSymbol>()
			.Where(namedSymbol => namedSymbol.AllInterfaces.Any(IsComponentInterface))
			.Select(namedSymbol =>
			{
				var isSingleton = namedSymbol.AllInterfaces.Any(IsSingletonComponentInterface);
				return new ComponentTypeDefinition(namedSymbol!.Name, namedSymbol.ContainingNamespace.ToString(),
					isSingleton);
			});

		var typeInfo = semanticModel.GetDeclaredSymbol(syntax)!;
		var name = typeInfo.Name;
		var ns = typeInfo.ContainingNamespace?.ToString();
		var classTypeDefinition = new TypeDefinition(name, ns);

		var iteratingSystemType = compilation.GetTypeByMetadataName(ITERATING_SYSTEM_METADATA_NAME);
		var isIteratingSystem = IsIteratingSystemClass(typeInfo.BaseType);
		return new IntermediateSyntax(classTypeDefinition, isIteratingSystem, allTypes);

		bool IsUtilityAttributeSyntax(AttributeSyntax attributeSyntax)
		{
			var symbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol;
			return SymbolEqualityComparer.Default.Equals(symbol?.ContainingType, attributeType);
		}

		bool IsComponentInterface(ISymbol symbol) => SymbolEqualityComparer.Default.Equals(symbol, componentType);

		bool IsSingletonComponentInterface(ISymbol symbol) =>
			SymbolEqualityComparer.Default.Equals(symbol, singletonComponentType);

		bool IsIteratingSystemClass(ITypeSymbol? symbol) =>
			symbol != null && (SymbolEqualityComparer.Default.Equals(symbol, iteratingSystemType) ||
			                   IsIteratingSystemClass(symbol.BaseType));
	}

	private static void Execute(Compilation compilation, ImmutableArray<IntermediateSyntax> classes,
		SourceProductionContext context)
	{
		var builder = new ClassBuilder();
		foreach (var ((className, classNamespace), isIteratingSystem, typesToGenerate) in classes)
		{
			var fullSystemName = $"{classNamespace}.{className}";
			var fileName = $"{fullSystemName.Replace(oldValue: ".", newValue: "_")}.generated.cs";

			builder.Clear();
			builder.BeginBlock($"namespace {classNamespace}");
			{
				builder.BeginBlock($"public partial class {className}");
				{
					foreach (var (componentName, componentNamespace, isSingleton) in typesToGenerate)
					{
						var mapperContext =
							builder.ComponentMapper(componentName, componentNamespace, isSingleton, isIteratingSystem);
						builder.UtilityMethods(mapperContext);
						var fullComponentName = $"{componentNamespace}.{componentName}";
						context.ReportMethodsGenerated(fileName, fullComponentName, fullSystemName);
					}
				}
				builder.EndBlock();
			}
			builder.EndBlock();

			context.AddSource(fileName, SourceText.From(builder.Generate(), Encoding.UTF8));
		}
	}

	private record TypeDefinition(string Name, string? Namespace);

	private record ComponentTypeDefinition(string Name, string? Namespace, bool IsSingleton) : TypeDefinition(Name,
		Namespace);

	private record IntermediateSyntax(TypeDefinition ClassDefinition, bool IsIteratingSystem,
		IEnumerable<ComponentTypeDefinition> TypesToGenerate);
}