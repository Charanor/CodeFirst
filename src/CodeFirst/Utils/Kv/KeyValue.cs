using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Antlr4.Runtime;
using CodeFirst.FileSystem;

namespace CodeFirst.Utils.Kv;

public class KeyValue
{
	public static readonly KeyValue Null = new();

	private readonly string stringValue = "";
	private readonly float numberValue;
	private readonly bool boolValue;

	private readonly Dictionary<string, KeyValue>? children;
	private KeyValue[]? listItems;

	public KeyValue()
	{
		Type = KeyValueType.Null;
	}

	public KeyValue(string value)
	{
		stringValue = value;
		Type = KeyValueType.String;
	}

	public KeyValue(int value) : this((float)value)
	{
	}

	public KeyValue(float value)
	{
		numberValue = value;
		Type = KeyValueType.Number;
	}

	public KeyValue(bool value)
	{
		boolValue = value;
		Type = KeyValueType.Boolean;
	}

	public KeyValue(params KeyValue[] values)
	{
		Type = KeyValueType.List;
		listItems = [..values];
	}

	public KeyValue(IDictionary<string, KeyValue> children)
	{
		this.children = children.ToDictionary();
		Type = KeyValueType.Object;
	}

	public KeyValueType Type { get; }

	public KeyValue this[string key] => GetChild(key);
	public KeyValue this[int key] => GetChild(key);

	public string AsString()
	{
		if (Type != KeyValueType.String)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.String)}");
		}

		return stringValue;
	}

	public float AsNumber()
	{
		if (Type != KeyValueType.Number)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.Number)}");
		}

		return numberValue;
	}

	public bool AsBoolean()
	{
		if (Type != KeyValueType.Boolean)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.Boolean)}");
		}

		return boolValue;
	}

	public IEnumerable<KeyValue> ListItems()
	{
		if (Type != KeyValueType.List)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.List)}");
		}

		Debug.Assert(listItems != null);
		return listItems;
	}

	public Dictionary<string, KeyValue> Children()
	{
		if (Type != KeyValueType.Object)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.List)}");
		}

		Debug.Assert(children != null);
		return children;
	}

	public KeyValue GetChild(string key)
	{
		if (Type != KeyValueType.Object)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.Object)}");
		}

		Debug.Assert(children != null);
		if (!children.TryGetValue(key, out var child))
		{
			throw new KeyNotFoundException($"No child found with key {key}.");
		}

		return child;
	}

	public KeyValue GetChild(int index)
	{
		if (Type != KeyValueType.List)
		{
			throw new InvalidCastException($"Cannot cast {Type} to {nameof(KeyValueType.List)}");
		}

		Debug.Assert(listItems != null);
		if (index < 0 || index >= listItems.Length)
		{
			throw new IndexOutOfRangeException($"0 <= {index} < {listItems.Length}");
		}

		return listItems[index];
	}

	public bool TryGetChild(string key, out KeyValue? child)
	{
		if (Type != KeyValueType.Object)
		{
			child = null;
			return false;
		}

		Debug.Assert(children != null);
		return children.TryGetValue(key, out child);
	}

	public void Extend(KeyValue kv)
	{
		if (Type != kv.Type)
		{
			DevTools.Throw<KeyValue>(new ArgumentException(
				$"Cannot extend KeyValue with a KeyValue of a different type! Parent is {Type} child is {kv.Type}."));
			return;
		}

		switch (Type)
		{
			case KeyValueType.Object:
				foreach (var (key, value) in kv.children!)
				{
					children![key] = value;
				}

				break;
			case KeyValueType.List:
				var extraSize = kv.listItems!.Length;
				Array.Resize(ref listItems!, listItems!.Length + extraSize);
				Array.Copy(kv.listItems!, sourceIndex: 0, listItems, listItems.Length, extraSize);
				break;
			default:
				DevTools.Throw<KeyValue>(new ArgumentException(
					$"Cannot extend KeyValue of type {Type}, only {KeyValueType.Object} or {KeyValueType.List} can be extended."));
				break;
		}
	}

	public static explicit operator string(KeyValue value) => value.AsString();
	public static explicit operator float(KeyValue value) => value.AsNumber();
	public static explicit operator bool(KeyValue value) => value.AsBoolean();

	protected bool Equals(KeyValue other)
	{
		if (Type != other.Type)
		{
			return false;
		}

		switch (Type)
		{
			case KeyValueType.Null:
				return true;
			case KeyValueType.String:
				return stringValue == other.stringValue;
			case KeyValueType.Number:
				return numberValue == other.numberValue;
			case KeyValueType.Boolean:
				return boolValue == other.boolValue;
			case KeyValueType.Object:
				Debug.Assert(children != null);
				Debug.Assert(other.children != null);
				foreach (var key in children.Keys.Concat(other.children.Keys))
				{
					if (!children.TryGetValue(key, out var kv1) ||
					    !other.children.TryGetValue(key, out var kv2) ||
					    kv1 != kv2)
					{
						return false;
					}
				}

				return true;
			case KeyValueType.List:
				Debug.Assert(listItems != null);
				Debug.Assert(other.listItems != null);
				if (listItems.Length != other.listItems.Length)
				{
					return false;
				}

				for (var i = 0; i < listItems.Length; i++)
				{
					if (listItems[i] != other.listItems[i])
					{
						return false;
					}
				}

				return true;
			default:
				return DevTools.DebugReturn<bool, KeyValue>(value: false, new ArgumentOutOfRangeException());
		}
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((KeyValue)obj);
	}

	public override int GetHashCode() =>
		HashCode.Combine(stringValue, numberValue, boolValue, children, listItems, (int)Type);

	public static bool operator ==(KeyValue? left, KeyValue? right) => Equals(left, right);

	public static bool operator !=(KeyValue? left, KeyValue? right) => !Equals(left, right);

	public override string ToString()
	{
		var sb = new StringBuilder();
		AppendSelf(sb);
		return sb.ToString();
	}

	private void AppendSelf(StringBuilder sb, int indent = 0)
	{
		switch (Type)
		{
			case KeyValueType.Null:
				sb.Append("pass");
				break;
			case KeyValueType.String:
				sb.Append(stringValue);
				break;
			case KeyValueType.Number:
				sb.Append(numberValue);
				break;
			case KeyValueType.Boolean:
				sb.Append(boolValue);
				break;
			case KeyValueType.Object:
				Debug.Assert(children != null);
				foreach (var (key, value) in children)
				{
					Indent();
					sb.Append(key);
					sb.Append(": ");

					if (value.Type == KeyValueType.Object)
					{
						sb.Append('\n');
					}

					value.AppendSelf(sb, indent + 1);
					sb.Append('\n');
				}

				break;
			case KeyValueType.List:
				Debug.Assert(listItems != null);
				sb.Append('[');
				for (var i = 0; i < listItems.Length; i++)
				{
					var item = listItems[i];
					if (item.Type is not (KeyValueType.String or KeyValueType.Number or KeyValueType.Boolean))
					{
						// Invalid list value
						DevTools.Throw<KeyValue>(new KeyValueParsingException($"Invalid list type: {item.Type}"));
						continue;
					}

					item.AppendSelf(sb, indent + 1);
					if (i < listItems.Length - 1)
					{
						sb.Append(", ");
					}
				}

				sb.Append(']');
				break;
			default:
				DevTools.Throw<KeyValue>(new ArgumentOutOfRangeException());
				break;
		}

		return;

		void Indent()
		{
			for (var i = 0; i < indent + 1; i++)
			{
				sb.Append('\t');
			}
		}
	}

	public static bool TryFromString(string kv, [NotNullWhen(true)] out KeyValue? asset)
	{
		var inputStream = new AntlrInputStream(kv);
		var lexer = new KeyValueLexer(inputStream);
		var commonTokenStream = new CommonTokenStream(lexer);
		var parser = new KeyValueParser(commonTokenStream);
		asset = new KeyValueVisitor().Visit(parser.program());
		return asset != null;
	}

	public static KeyValue FromString(string kv)
	{
		var inputStream = new AntlrInputStream(kv);
		var lexer = new KeyValueLexer(inputStream);
		var commonTokenStream = new CommonTokenStream(lexer);
		var parser = new KeyValueParser(commonTokenStream);
		return new KeyValueVisitor().Visit(parser.program());
	}
}