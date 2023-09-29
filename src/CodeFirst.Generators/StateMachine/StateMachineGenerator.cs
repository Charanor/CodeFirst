using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeFirst.Generators.StateMachine;

[Generator]
public class StateMachineGenerator : IIncrementalGenerator
{
	private const string STATE_MACHINE_ATTRIBUTE_NAMESPACE = "CodeFirst.Ai.State.Attributes.Generation";
	private const string STATE_MACHINE_ATTRIBUTE_NAME_SIMPLE = "StateMachine";
	private const string STATE_MACHINE_ATTRIBUTE_NAME = $"{STATE_MACHINE_ATTRIBUTE_NAME_SIMPLE}Attribute";

	private const string STATE_MACHINE_CLASS = "CodeFirst.Ai.State.StateMachine";

	private const string STATE_MACHINE_ATTRIBUTE =
		$"{STATE_MACHINE_ATTRIBUTE_NAMESPACE}.{STATE_MACHINE_ATTRIBUTE_NAME}";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
			STATE_MACHINE_ATTRIBUTE,
			static (syntaxNode, _) => IsEnumDeclaration(syntaxNode),
			static (ctx, _) => CreateStateMachineIntermediateSyntax(ctx)
		).Where(static syntax => syntax is not null);

		var compilationSystems = context.CompilationProvider.Combine(declarations.Collect());
		context.RegisterSourceOutput(compilationSystems,
			static (spc, source) => Execute(
				source.Left,
				source.Right.CastArray<StateMachineIntermediateSyntax>(),
				spc
			)
		);
	}

	private static bool IsEnumDeclaration(SyntaxNode node) => node is EnumDeclarationSyntax;

	private static StateMachineIntermediateSyntax? CreateStateMachineIntermediateSyntax(
		GeneratorAttributeSyntaxContext context)
	{
		var syntax = (EnumDeclarationSyntax)context.TargetNode;
		var typeInfo = context.SemanticModel.GetDeclaredSymbol(syntax);
		if (typeInfo == null)
		{
			return null;
		}

		var memberNames = syntax.Members.Select(member => member.Identifier.ToString()).ToList();
		return new StateMachineIntermediateSyntax(typeInfo.Name, typeInfo.ContainingNamespace.ToString(), memberNames);
	}

	private static void Execute(Compilation compilation,
		ImmutableArray<StateMachineIntermediateSyntax> stateMachineEnums,
		SourceProductionContext context)
	{
		if (stateMachineEnums.Length == 0)
		{
			return;
		}

		var builder = new ClassBuilder();
		foreach (var (enumIdentifier, enumNamespace, memberNames) in stateMachineEnums)
		{
			builder.Clear();
			using (builder.Block($"namespace {enumNamespace}"))
			using (builder.Block(
				       $"public partial class {enumIdentifier}Machine : {STATE_MACHINE_CLASS}<{enumIdentifier}>"))
			{
				using (builder.Block("public override void Process(float delta)"))
				{
					builder.IndentedLn("ProcessGlobal(delta);");
					using (builder.Block("switch (State)"))
					{
						foreach (var memberName in memberNames)
						{
							using (builder.Block($"case {enumIdentifier}.{memberName}: "))
							{
								builder.IndentedLn($"State = {ProcessMethod(memberName)}();");
								builder.IndentedLn("break;");
							}
						}
					}
				}

				builder.NewLine();

				builder.IndentedLn("partial void ProcessGlobal(float delta);");
				foreach (var memberName in memberNames)
				{
					builder.IndentedLn($"private partial {enumIdentifier} {ProcessMethod(memberName)}();");
				}

				builder.NewLine();

				using (builder.Block($"protected override void OnStateEnter({enumIdentifier} previousState)"))
				{
					using (builder.Block("switch (State)"))
					{
						foreach (var memberName in memberNames)
						{
							using (builder.Block($"case {enumIdentifier}.{memberName}: "))
							{
								builder.IndentedLn($"{StateEnterMethod(memberName)}(previousState);");
								builder.IndentedLn("break;");
							}
						}
					}
				}

				builder.NewLine();

				foreach (var memberName in memberNames)
				{
					builder.IndentedLn(
						$"partial void {StateEnterMethod(memberName)}({enumIdentifier} previousState);");
				}

				builder.NewLine();

				using (builder.Block($"protected override void OnStateExit({enumIdentifier} upcomingState)"))
				{
					using (builder.Block("switch (State)"))
					{
						foreach (var memberName in memberNames)
						{
							using (builder.Block($"case {enumIdentifier}.{memberName}: "))
							{
								builder.IndentedLn($"{StateExitMethod(memberName)}(upcomingState);");
								builder.IndentedLn("break;");
							}
						}
					}
				}

				builder.NewLine();

				foreach (var memberName in memberNames)
				{
					builder.IndentedLn(
						$"partial void {StateExitMethod(memberName)}({enumIdentifier} upcomingState);");
				}
			}

			context.AddSource($"{enumIdentifier}Machine.g.cs", SourceText.From(builder.Generate(), Encoding.UTF8));
		}
	}

	private static string ProcessMethod(string stateName) => $"Process{stateName}";
	private static string StateEnterMethod(string stateName) => $"On{stateName}Enter";
	private static string StateExitMethod(string stateName) => $"On{stateName}Exit";
}

public record StateMachineIntermediateSyntax(string EnumIdentifier, string EnumNamespace, IList<string> MemberNames);