using System.Diagnostics;
using System.Globalization;

namespace CodeFirst.Utils.Kv;

public class KeyValueVisitor : KeyValueBaseVisitor<KeyValue>
{
	// protected override KeyValue DefaultResult => KeyValue.Null;

	public override KeyValue VisitProgram(KeyValueParser.ProgramContext context)
	{
		var definitions = new Dictionary<string, List<string>>();
		var definitionList = context.definitions().definition();
		foreach (var definition in definitionList)
		{
			var key = definition.Identifier().GetText();
			var value = definition.definitionValue().GetText();
			if (value.StartsWith('\"') && value.EndsWith('\"'))
			{
				value = value.Substring(startIndex: 1, value.Length - 2);
			}

			if (!definitions.TryGetValue(key, out var list))
			{
				list = new List<string>();
				definitions[key] = list;
			}

			list.Add(value);
		}

		var children = new Dictionary<string, KeyValue>();
		var keyValues = context.kv();
		if (keyValues == null)
		{
			return new KeyValueRoot(children, definitions.ToDictionary(k => k.Key, v => v.Value.ToArray()));
		}

		foreach (var kv in keyValues)
		{
			var key = kv.Identifier().GetText();
			if (children.ContainsKey(key))
			{
				DevTools.Throw<KeyValueVisitor>(new KeyValueParsingException($"Duplicate key: {key}"));
				continue;
			}

			var value = kv.value();
			var kvValue = value.PASS() != null ? KeyValue.Null : value.Accept(this);
			children.Add(key, kvValue);
		}

		return new KeyValueRoot(children, definitions.ToDictionary(k => k.Key, v => v.Value.ToArray()));
	}

	public override KeyValue VisitPrimitiveValue(KeyValueParser.PrimitiveValueContext context)
	{
		if (context.Boolean() != null)
		{
			var boolean = bool.Parse(context.Boolean().GetText());
			return new KeyValue(boolean);
		}

		if (context.Number() != null)
		{
			var number = float.Parse(context.Number().GetText(), CultureInfo.InvariantCulture);
			return new KeyValue(number);
		}

		// it's a string!
		var text = context.String().GetText()[1..^1]; // Remove quotes at start and end
		return new KeyValue(text);
	}

	public override KeyValue VisitChildren(KeyValueParser.ChildrenContext context)
	{
		var children = new Dictionary<string, KeyValue>();

		var keyValues = context.kv();
		if (keyValues == null)
		{
			return new KeyValue(children);
		}

		foreach (var kv in keyValues)
		{
			var key = kv.Identifier().GetText();
			if (children.ContainsKey(key))
			{
				DevTools.Throw<KeyValueVisitor>(new KeyValueParsingException($"Duplicate key: {key}"));
				continue;
			}

			var value = kv.value();
			children.Add(key, value.PASS() != null ? KeyValue.Null : value.Accept(this));
		}

		return new KeyValue(children);
	}

	public override KeyValue VisitValueList(KeyValueParser.ValueListContext context)
	{
		var items = new List<KeyValue>();

		for (var chain = context.valueListChain(); chain != null; chain = chain.valueListChain())
		{
			items.Add(chain.primitiveValue().Accept(this));
		}

		return new KeyValue(items.ToArray());
	}
}

public class KeyValueParsingException(string message) : Exception(message);