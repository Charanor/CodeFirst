using System.Collections.Generic;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace JXS.Graphics.Generators.Generators;

public static class Diagnostics
{
	private static readonly DiagnosticDescriptor UnknownShaderType = new(
		id: "SRG0001",
		title: "Unknown shader type",
		messageFormat: "Could not determine shader type. Expected file ending to be one of: {0}",
		category: "Shaders",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor IllegalInlineStructSpecifier = new(
		id: "SRG0002",
		title: "Illegal inline struct specifier",
		messageFormat:
		"Shader contains an inline struct specifier. Inline struct specifiers can not be processed, please extract the struct specifier to a named struct in the outer scope.",
		category: "Shaders",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static SourceProductionContext SourceProductionContext { get; set; }

	public static void ReportUnknownShaderType(string shaderFilePath, IEnumerable<string> validShaderFileEndings)
	{
		var textSpan = TextSpan.FromBounds(start: -1, end: -1);
		var linePositionSpan = new LinePositionSpan(LinePosition.Zero, LinePosition.Zero);
		var location = Location.Create(shaderFilePath, textSpan, linePositionSpan);
		var fileEndingsArrayText = $"[{string.Join(separator: ",", validShaderFileEndings)}]";
		SourceProductionContext.ReportDiagnostic(Diagnostic.Create(UnknownShaderType, location, fileEndingsArrayText));
	}

	public static void ReportIllegalInlineStructSpecifier(string shaderFilePath, ParserRuleContext context)
	{
		var textSpan = GetTextSpan(context);
		var lineSpan = GetLineSpan(context);
		var location = Location.Create(shaderFilePath, textSpan, lineSpan);
		var diagnostic = Diagnostic.Create(IllegalInlineStructSpecifier, location);
		SourceProductionContext.ReportDiagnostic(diagnostic);
	}

	private static TextSpan GetTextSpan(ParserRuleContext context) =>
		TextSpan.FromBounds(context.Start.TokenIndex, context.Stop.TokenIndex);

	private static LinePositionSpan GetLineSpan(ParserRuleContext context)
	{
		var start = context.Start;
		var startLine = new LinePosition(start.Line, start.Column);

		var stop = context.Stop;
		var endLine = new LinePosition(stop.Line, stop.Column);

		return new LinePositionSpan(startLine, endLine);
	}
}