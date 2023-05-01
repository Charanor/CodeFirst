using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Ecs.Generators.Parsing;
using Ecs.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Ecs.Generators.Generators;

[Generator]
public class GenerateDefinitions : IIncrementalGenerator
{
	private const string FILE_EXTENSION = ".ecs";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// if (!Debugger.IsAttached)
		// {
		// 	Debugger.Launch();
		// }

		var files = context.AdditionalTextsProvider
			.Where(static file => Path.GetExtension(file.Path) == FILE_EXTENSION)
			.Select(static (file, ct) =>
			{
				var fileName = Path.GetFileNameWithoutExtension(file.Path);
				var contents = file.GetText(ct)?.ToString();
				return contents == null ? null : new FileInfo(fileName, contents);
			})
			.Where(static info => info != null);

		var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());
		context.RegisterSourceOutput(compilationAndFiles, Execute);
	}

	private static void Execute(SourceProductionContext sourceProductionContext,
		(Compilation Compilation, ImmutableArray<FileInfo?> Files) source)
	{
		Execute(source.Files.CastArray<FileInfo>(), sourceProductionContext, source.Compilation);
	}

	private static void Execute(ImmutableArray<FileInfo> definitions, SourceProductionContext context,
		Compilation compilation)
	{
		if (definitions.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (var (fileName, definition) in definitions)
		{
			var inputStream = new AntlrInputStream(definition);
			var lexer = new EcsLexer(inputStream);
			var commonTokenStream = new CommonTokenStream(lexer);
			var parser = new EcsParser(commonTokenStream)
			{
			};
			parser.AddErrorListener(new DebugErrorListener(context, fileName));
			var visitor = new EcsVisitor();
			var programContext = parser.program();
			var program = visitor.Visit(programContext).OfType<EcsProgram>().FirstOrDefault();
			if (program == null)
			{
				// TODO: Error
				continue;
			}

			var source = new EcsDefinitionGenerator(program, compilation).GenerateSource();
			context.AddSource($"{fileName}.generated.cs", SourceText.From(source, Encoding.UTF8));
		}
	}
}

public record FileInfo(string FileName, string Contents);

public class DebugErrorListener : IAntlrErrorListener<IToken>
{
	private readonly SourceProductionContext context;
	private readonly string fileName;

	public DebugErrorListener(SourceProductionContext context, string fileName)
	{
		this.context = context;
		this.fileName = fileName;
	}

	public void SyntaxError(
		TextWriter output,
		IRecognizer recognizer,
		IToken offendingSymbol,
		int line,
		int charPositionInLine,
		string msg,
		RecognitionException e)
	{
		var format = $"line {line}:{charPositionInLine} {msg}";
		output.WriteLine(format);
		Debug.WriteLine(format);
		context.ReportParsingError(fileName, offendingSymbol, line, charPositionInLine, msg);
	}
}