using System;
using System.Text;

namespace JXS.SourceGeneratorUtils;

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

	public void EndBlock(string suffix = "")
	{
		Dedent();
		IndentedLn($"}}{suffix}");
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

	public void Clear() => sb.Clear();

	public void NewLine() => Raw(Environment.NewLine);

	public IDisposable Block(string text) => new DisposableBlock(text, this);

	private record DisposableBlock : IDisposable
	{
		private readonly ClassBuilder builder;

		public DisposableBlock(string text, ClassBuilder builder)
		{
			this.builder = builder;
			builder.BeginBlock(text);
		}

		public void Dispose()
		{
			builder.EndBlock();
		}
	}
}