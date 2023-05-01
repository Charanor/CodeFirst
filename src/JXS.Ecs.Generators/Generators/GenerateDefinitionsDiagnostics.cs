using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

public static class GenerateDefinitionsDiagnostics
{
	private static readonly DiagnosticDescriptor ParsingError = new(
		id: "DRG0101",
		title: "Parsing error",
		messageFormat: "Parsing failed at {0}:{1} {2}",
		category: "Antlr",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static void ReportParsingError(this SourceProductionContext ctx, string filePath, IToken offendingSymbol,
		int line, int column, string message)
	{
		var textSpan = new TextSpan(offendingSymbol.StartIndex, offendingSymbol.StopIndex - offendingSymbol.StartIndex);
		var linePositionSpan = new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column));
		var location = Location.Create(filePath, textSpan, linePositionSpan);
		ctx.ReportDiagnostic(Diagnostic.Create(ParsingError, location, line, column, message));
	}
}