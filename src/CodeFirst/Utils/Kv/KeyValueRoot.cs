using CodeFirst.FileSystem;

namespace CodeFirst.Utils.Kv;

public class KeyValueRoot : KeyValue
{
	public KeyValueRoot(IReadOnlyDictionary<string, string[]> definitions)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(string value, IReadOnlyDictionary<string, string[]> definitions) : base(value)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(int value, IReadOnlyDictionary<string, string[]> definitions) : base(value)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(float value, IReadOnlyDictionary<string, string[]> definitions) : base(value)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(bool value, IReadOnlyDictionary<string, string[]> definitions) : base(value)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(KeyValue[] values, IReadOnlyDictionary<string, string[]> definitions) : base(values)
	{
		Definitions = definitions;
	}

	public KeyValueRoot(IDictionary<string, KeyValue> children, IReadOnlyDictionary<string, string[]> definitions) : base(children)
	{
		Definitions = definitions;
	}

	public IReadOnlyDictionary<string, string[]> Definitions { get; }
	public FileHandle? Source { get; internal set; }
}