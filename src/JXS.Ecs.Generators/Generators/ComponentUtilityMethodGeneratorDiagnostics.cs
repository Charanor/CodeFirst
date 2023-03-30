using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

public static class ComponentUtilityMethodGeneratorDiagnostics
{
	private static readonly DiagnosticDescriptor MethodsGenerated = new(
		id: "DRG0001",
		title: "Invalid XML file",
		messageFormat: "Generated utility methods for component {0} in system {1}",
		category: "Prefabs",
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public static void ReportMethodsGenerated(this SourceProductionContext ctx, string filePath,
		string fullComponentName, string fullSystemName)
	{
		var textSpan = new TextSpan(start: 0, length: 0);
		var linePositionSpan = new LinePositionSpan(LinePosition.Zero, LinePosition.Zero);
		var location = Location.Create(filePath, textSpan, linePositionSpan);
		ctx.ReportDiagnostic(Diagnostic.Create(MethodsGenerated, location, fullComponentName, fullSystemName));
	}
}