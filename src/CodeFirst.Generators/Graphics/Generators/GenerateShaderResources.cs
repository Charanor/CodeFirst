using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using CodeFirst.Generators.Graphics.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CodeFirst.Generators.Graphics.Generators;

[Generator]
public class GenerateShaderResources : IIncrementalGenerator
{
	private const string VERTEX_FILE_ENDING = ".vert";
	private const string FRAGMENT_FILE_ENDING = ".frag";

	private static readonly ImmutableArray<string> ValidShaderFileEndings =
		new[] { VERTEX_FILE_ENDING, FRAGMENT_FILE_ENDING }.ToImmutableArray();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		if (!Debugger.IsAttached)
		{
			// Debugger.Launch();
		}

		var files = context.AdditionalTextsProvider
			.Where(static file => ValidShaderFileEndings.Contains(Path.GetExtension(file.Path)))
			.Select(static (file, cancellationToken) =>
			{
				var fileName = Path.GetFileNameWithoutExtension(file.Path);
				var fileExtension = Path.GetExtension(file.Path);
				var fileText = file.GetText(cancellationToken)?.ToString();
				return fileText is null ? null : new ShaderFileInfo(file.Path, fileName, fileExtension, fileText);
			})
			.Where(static info => info is not null);

		var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());
		context.RegisterSourceOutput(compilationAndFiles,
			static (sourceProductionContext, source) =>
				Execute(source.Right.CastArray<ShaderFileInfo>(), sourceProductionContext));
	}

	private static void Execute(ImmutableArray<ShaderFileInfo> fileInfos, SourceProductionContext context)
	{
		if (fileInfos.IsDefaultOrEmpty)
		{
			return;
		}

		Diagnostics.SourceProductionContext = context;

		var shaderGroups = new Dictionary<string, IList<ShaderFile>>();
		foreach (var (filePath, fileName, fileExtension, fileText) in fileInfos)
		{
			var shaderType = GetShaderType(fileExtension);
			if (shaderType is ShaderType.Unknown)
			{
				Diagnostics.ReportUnknownShaderType(fileName, ValidShaderFileEndings);
				continue;
			}

			var shaderName = Path.GetFileNameWithoutExtension(fileName);
			if (!shaderGroups.TryGetValue(shaderName, out var shaderFiles))
			{
				shaderFiles = new List<ShaderFile>();
				shaderGroups.Add(shaderName, shaderFiles);
			}

			shaderFiles.Add(new ShaderFile(filePath, shaderType, fileText));
		}

		foreach (var entry in shaderGroups)
		{
			var className = $"{entry.Key}Shader";
			var shaderFiles = entry.Value;

			var definitions = shaderFiles.SelectMany(shaderFile =>
			{
				var (_, shaderType, shaderSource) = shaderFile;
				var inputStream = new AntlrInputStream(shaderSource);
				var lexer = new GLSLLexer(inputStream);
				var commonTokenStream = new CommonTokenStream(lexer);
				var parser = new GLSLParser(commonTokenStream);
				var visitor = new ShaderVisitor(shaderType);
				return visitor.Visit(parser.translation_unit()).Append(new GLSLShaderSource(shaderType, shaderSource));
			});

			var classSource = new ShaderClassGenerator(className, definitions).GenerateClassSource();
			context.AddSource($"{className}.g.cs", SourceText.From(classSource, Encoding.UTF8));
		}
	}

	private static ShaderType GetShaderType(string shaderExt) => shaderExt switch
	{
		VERTEX_FILE_ENDING => ShaderType.Vertex,
		FRAGMENT_FILE_ENDING => ShaderType.Fragment,
		_ => ShaderType.Unknown
	};

	private record ShaderFileInfo(string FilePath, string FileName, string FileExtension, string FileText);
}