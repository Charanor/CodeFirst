using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeFirst.Generators.Ecs.Generators;

internal record ProcessEntityIntermediateSyntax(
	ClassDeclarationSyntax ClassSyntax,
	MethodDeclarationSyntax MethodSyntax
);