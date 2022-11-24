using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ecs.Generators.Generators;

internal record IntermediateSyntax(ClassDeclarationSyntax ClassSyntax, MethodDeclarationSyntax MethodSyntax);