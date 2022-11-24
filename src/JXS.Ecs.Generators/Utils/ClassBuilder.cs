using System;
using System.Text;

namespace Ecs.Generators.Utils;

public class ClassBuilder
{
	private readonly StringBuilder sb;
	private int indentation;
	private int docstringIndentation;

	public ClassBuilder()
	{
		sb = new StringBuilder();
		indentation = 0;
		docstringIndentation = 0;
	}

	public string Generate() => sb.ToString();

	public override string ToString() => Generate();

	public void Indent() => indentation++;

	public void Dedent() => indentation--;

	public void BeginBlock(string statement = "")
	{
		if (statement.Length != 0)
		{
			IndentedLn(statement);
		}

		IndentedLn("{");
		Indent();
	}

	public void EndBlock()
	{
		Dedent();
		IndentedLn("}");
	}

	public void Raw(string text) => sb.Append(text);

	public void Indented(string code)
	{
		for (var i = 0; i < indentation; i++)
		{
			Raw("\t");
		}

		Raw(code);
	}

	/// <summary>
	///     asdasd
	/// </summary>
	/// <param name="code"></param>
	public void IndentedLn(string code) => Indented(code + Environment.NewLine);

	private void IndentDocstring() => docstringIndentation++;
	private void DedentDocstring() => docstringIndentation--;

	public void DocstringBlock(string tag, string contents)
	{
		DocstringLn($"<{tag}>");
		IndentDocstring();
		{
			DocstringLn(contents);
		}
		DedentDocstring();
		DocstringLn($"</{tag}>");
	}

	public void Docstring(string contents)
	{
		Indented("///");
		// <= SIC!
		for (var i = 0; i <= docstringIndentation; i++)
		{
			Raw("\t");
		}

		Raw(contents);
	}

	public void DocstringLn(string contents) => Docstring(contents + Environment.NewLine);
}