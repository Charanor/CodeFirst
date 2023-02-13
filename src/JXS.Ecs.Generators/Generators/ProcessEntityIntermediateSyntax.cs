using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ecs.Generators.Generators;

internal record ProcessEntityIntermediateSyntax(
	ClassDeclarationSyntax ClassSyntax,
	MethodDeclarationSyntax MethodSyntax
);