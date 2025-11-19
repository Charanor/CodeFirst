using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using CodeFirst.AssetManagement;
using CodeFirst.FileSystem;

namespace CodeFirst.Utils.Kv;

public class KeyValueAssetResolver : IAssetResolver
{
	private readonly KeyValueVisitor visitor = new();

	public bool CanLoadAssetOfType(Type type) => type == typeof(KeyValue) || type == typeof(KeyValueRoot);

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out object? asset)
	{
		if (fileHandle.Extension != ".kv")
		{
			asset = null;
			return false;
		}

		var result = TryLoadAsset(fileHandle, out KeyValueRoot? kv);
		asset = kv;
		return result;
	}

	public bool TryLoadAsset(FileHandle fileHandle, [NotNullWhen(true)] out KeyValueRoot? asset)
	{
		using var stream = fileHandle.Read();
		var inputStream = new AntlrInputStream(stream);
		var lexer = new KeyValueLexer(inputStream);
		var commonTokenStream = new CommonTokenStream(lexer);
		var parser = new KeyValueParser(commonTokenStream);
		asset = visitor.Visit(parser.program()) as KeyValueRoot;
		if (asset != null)
		{
			asset.Source = fileHandle;
		}

		return asset != null;
	}
}