using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Ecs.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

[Generator]
public class GenerateProcessEntityMethod : IIncrementalGenerator
{
	private const string ITERATING_SYSTEM_NAME = "IteratingSystem";

	private const string ENTITY_PROCESSOR_NAMESPACE = "JXS.Ecs.Core.Attributes.Generation";
	private const string ENTITY_PROCESSOR_NAME = "EntityProcessorAttribute";

	private const string ENTITY_PROCESSOR_FULL_NAME =
		$"{ENTITY_PROCESSOR_NAMESPACE}.{ENTITY_PROCESSOR_NAME}";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var declarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (syntaxNode, _) => IsEntitySystem(syntaxNode),
				transform: static (ctx, _) => TransformSyntaxToEntitySystemSyntax(ctx)
			)
			.Where(static syntax => syntax is not null);

		var compilationSystems = context.CompilationProvider.Combine(declarations.Collect());
		context.RegisterSourceOutput(
			compilationSystems,
			action: static (spc, source) =>
				Execute(
					source.Item1,
					source.Item2.CastArray<ProcessEntityIntermediateSyntax>(),
					spc
				)
		);
	}

	private static bool IsEntitySystem(SyntaxNode node)
	{
		if (
			node
			is not ClassDeclarationSyntax
			{
				AttributeLists.Count: > 0,
				BaseList.Types.Count: > 0
			} syntax
		)
		{
			return false;
		}

		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach (var memberDeclarationSyntax in syntax.Members)
		{
			if (memberDeclarationSyntax is MethodDeclarationSyntax { AttributeLists.Count: > 0 })
			{
				return true;
			}
		}

		return true;
	}

	private static ProcessEntityIntermediateSyntax? TransformSyntaxToEntitySystemSyntax(
		GeneratorSyntaxContext context
	)
	{
		var syntax = (ClassDeclarationSyntax)context.Node;

		var isEntitySystem = false;
		foreach (var baseTypeSyntax in syntax.BaseList!.Types)
		{
			if (
				baseTypeSyntax.Type is not IdentifierNameSyntax stx
				|| stx.Identifier.ToString() != ITERATING_SYSTEM_NAME
			)
			{
				continue;
			}

			isEntitySystem = true;
			break;
		}

		if (!isEntitySystem)
		{
			return null;
		}

		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach (var memberDeclarationSyntax in syntax.Members)
		{
			if (
				memberDeclarationSyntax
				is not MethodDeclarationSyntax
				{
					ParameterList.Parameters.Count: > 0,
					AttributeLists.Count: > 0
				} methodSyntax
			)
			{
				continue;
			}

			foreach (var attributeListSyntax in methodSyntax.AttributeLists)
			{
				foreach (var attributeSyntax in attributeListSyntax.Attributes)
				{
					if (
						context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol
						is not IMethodSymbol attributeSymbol
					)
					{
						continue;
					}

					var namedTypeNode = attributeSymbol.ContainingType;
					var fullName = namedTypeNode.ToDisplayString();
					if (fullName == ENTITY_PROCESSOR_FULL_NAME)
					{
						return new ProcessEntityIntermediateSyntax(syntax, methodSyntax);
					}
				}
			}
		}

		return null;
	}

	private static void Execute(
		Compilation compilation,
		ImmutableArray<ProcessEntityIntermediateSyntax> systems,
		SourceProductionContext context
	)
	{
		if (systems.IsDefaultOrEmpty)
		{
			return;
		}

		var systemsToGenerate = GetSystemsToGenerate(
			compilation,
			systems,
			context.CancellationToken
		);
		if (systemsToGenerate.Count <= 0)
		{
			return;
		}

		var result = GenerationUtils.GenerateSystemClasses(systemsToGenerate.ToImmutableList());
		context.AddSource(
			hintName: "GeneratedSystems.g.cs",
			SourceText.From(result, Encoding.UTF8)
		);
	}

	private static IList<SystemToGenerate> GetSystemsToGenerate(
		Compilation compilation,
		IEnumerable<ProcessEntityIntermediateSyntax> syntaxes,
		CancellationToken ct
	)
	{
		var systemsToGenerate = new List<SystemToGenerate>();

		foreach (var (classSyntax, methodSyntax) in syntaxes)
		{
			ct.ThrowIfCancellationRequested();

			string? ns = null;
			if (classSyntax.Parent is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
			{
				ns = namespaceDeclarationSyntax.Name.ToString();
			}

			if (GetClassName(compilation, classSyntax) is not { } className)
			{
				continue;
			}

			if (GetMethodName(compilation, methodSyntax) is not { } methodName)
			{
				continue;
			}

			var parameters = new List<ParameterDeclaration>();
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var parameterSyntax in methodSyntax.ParameterList.Parameters)
			{
				var symbol = compilation
					.GetSemanticModel(parameterSyntax.SyntaxTree)
					.GetDeclaredSymbol(parameterSyntax);
				if (symbol is not IParameterSymbol parameterSymbol)
				{
					continue;
				}

				var modifier =
					parameterSyntax.Modifiers.Count > 0
						? parameterSyntax.Modifiers[0].ToString()
						: null;
				var fullType = parameterSymbol.Type.ToString();
				var simpleType = fullType;
				string? parameterNs = null;
				var optional = IsOptional(parameterSyntax);
				var idx = fullType.LastIndexOf('.');
				if (idx >= 0)
				{
					var hasQuestionMark = fullType.EndsWith("?");
					parameterNs = fullType.Substring(startIndex: 0, idx);
					simpleType = fullType.Substring(
						idx + 1,
						fullType.Length - (idx + 1) - (hasQuestionMark ? 1 : 0)
					);
				}

				var parameterName = parameterSyntax.Identifier.ToString();
				parameters.Add(
					new ParameterDeclaration(
						modifier,
						simpleType,
						parameterName,
						parameterNs,
						optional
					)
				);
			}

			if (parameters.Count <= 0)
			{
				continue;
			}

			systemsToGenerate.Add(
				new SystemToGenerate(ns, className, methodName, parameters.ToImmutableArray())
			);
		}

		return systemsToGenerate;
	}

	private static bool IsOptional(BaseParameterSyntax parameterSyntax)
	{
		return parameterSyntax.AttributeLists.SelectMany(list => list.Attributes)
			.Select(attribute => attribute.Name.ToString())
			.Any(name => name == "Optional" || name.EndsWith(".Optional"));
	}

	private static string? GetClassName(Compilation compilation, SyntaxNode syntax)
	{
		var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
		return semanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol classSymbol
			? null
			: classSymbol.Name;
	}

	private static string? GetMethodName(Compilation compilation, SyntaxNode syntax)
	{
		var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
		return semanticModel.GetDeclaredSymbol(syntax) is not IMethodSymbol methodSymbol
			? null
			: methodSymbol.Name;
	}
}