using System;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

public static class GenerateDefinitionsDiagnostics
{
	private static readonly DiagnosticDescriptor ParsingError = new(
		"DRG0101",
		"Parsing error",
		"Parsing failed at {0}:{1} {2}",
		"Antlr",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public static void ReportParsingError(this SourceProductionContext ctx, string filePath, IToken offendingSymbol,
		int line, int column, string message)
	{
		var start = offendingSymbol.StartIndex == -1 ? 0 : offendingSymbol.StartIndex;
		var end = Math.Max(val1: 0, offendingSymbol.StopIndex == -1 ? 0 : offendingSymbol.StopIndex - start);
		var textSpan = new TextSpan(start, end);
		var linePositionSpan =
			new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column + end));
		var location = Location.Create(filePath, textSpan, linePositionSpan);
		ctx.ReportDiagnostic(Diagnostic.Create(ParsingError, location, line, column, message));
	}
}