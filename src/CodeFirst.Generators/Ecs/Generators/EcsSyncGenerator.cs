using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeFirst.Generators.Ecs.Generators;

[Generator]
public class EcsSyncGenerator : IIncrementalGenerator
{
	private const string ATTRIBUTE_NAMESPACE = "CodeFirst.Ecs.Core.Attributes.Generation";
	private const string ATTRIBUTE_NAME_SIMPLE = "EcsSync";
	private const string ATTRIBUTE_NAME = $"{ATTRIBUTE_NAME_SIMPLE}Attribute";
	private const string FULL_ATTRIBUTE = $"{ATTRIBUTE_NAMESPACE}.{ATTRIBUTE_NAME}";

	private const string NOT_EDITOR_BROWSABLE =
		"[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
			FULL_ATTRIBUTE,
			static (syntaxNode, _) => IsComponentProperty(syntaxNode),
			static (ctx, _) => CreateGeneratorData(ctx)
		).Where(static syntax => syntax is not null);

		var compilationSystems = context.CompilationProvider.Combine(declarations.Collect());
		context.RegisterSourceOutput(compilationSystems,
			static (spc, source) => Execute(
				source.Left,
				source.Right.CastArray<GeneratorData>(),
				spc
			)
		);
	}

	private static bool IsComponentProperty(SyntaxNode node) => node is PropertyDeclarationSyntax
	{
		Parent: StructDeclarationSyntax
	};

	private static GeneratorData? CreateGeneratorData(GeneratorAttributeSyntaxContext context)
	{
		var syntax = (PropertyDeclarationSyntax)context.TargetNode;
		if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IPropertySymbol typeInfo)
		{
			return null;
		}

		var type = typeInfo.Type;
		var parent = typeInfo.ContainingType;
		var ns = typeInfo.ContainingNamespace;
		return new GeneratorData(typeInfo.Name, type.ToString(), parent.Name, ns.ToString());
	}

	private static void Execute(Compilation _, ImmutableArray<GeneratorData> data, SourceProductionContext ctx)
	{
		if (data.Length == 0)
		{
			return;
		}

		var builder = new ClassBuilder();
		var clumpedData = data.GroupBy(d => d.Parent);
		foreach (var group in clumpedData)
		{
			var component = group.Key;
			var ns = group.First().Namespace;
			using (builder.Block($"namespace {ns}"))
			{
				// We need this or the compiler will complain. The warning is not relevant for us however since we
				// do not use these structs to interop with native code.
				builder.IndentedLn("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]");
				using (builder.Block($"public partial struct {component}"))
				{
					foreach (var (name, type, _, _) in group)
					{
						if (!IsBool(type))
						{
							// Create a variable that tracks if the value has changed or not. We don't do this for bools
							// since a bool only takes up 1 bit of space so tracking dirty state is pointless.
							//
							// The property needs to be public so we can check it to prevent unnecessary serialization,
							// but it's not a useful property for the user so we make it not browsable in intellisense.
							builder.IndentedLn(NOT_EDITOR_BROWSABLE);
							builder.IndentedLn($"public bool {name}IsDirty {{ get; private set; }}");
						}

						using (builder.Block($"public partial {type} {name}"))
						{
							builder.IndentedLn("get => field;");
							using (builder.Block("set"))
							{
								if (!IsBool(type))
								{
									using (builder.Block("if (value == field)"))
									{
										builder.IndentedLn("return;");
									}

									builder.IndentedLn($"{name}IsDirty = true;");
								}

								builder.IndentedLn("field = value;");
							}
						}
					}
				}
			}
		}

		ctx.AddSource("EcsSync.g.cs", SourceText.From(builder.Generate(), Encoding.UTF8));
	}

	private static bool IsBool(string type) => type is "bool" or nameof(Boolean);

	private record GeneratorData(string Name, string Type, string Parent, string Namespace);
}