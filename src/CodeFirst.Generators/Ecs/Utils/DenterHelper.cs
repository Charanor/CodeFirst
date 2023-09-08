using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace CodeFirst.Generators.Ecs.Utils;

public abstract class DenterHelper
{
	private readonly int dedentToken;
	private readonly Queue<IToken> dentsBuffer = new();
	private readonly Stack<int> indentations = new();
	private readonly int indentToken;
	private readonly int nlToken;
	private IEofHandler eofHandler;
	private bool reachedEof;

	protected DenterHelper(int nlToken, int indentToken, int dedentToken)
	{
		this.nlToken = nlToken;
		this.indentToken = indentToken;
		this.dedentToken = dedentToken;
		eofHandler = new StandardEofHandler(this);
	}

	public IToken NextToken()
	{
		InitIfFirstRun();
		var t = dentsBuffer.Count == 0
			? PullToken()
			: dentsBuffer.Dequeue();
		if (reachedEof)
		{
			return t;
		}

		IToken r;
		if (t.Type == nlToken)
		{
			r = HandleNewlineToken(t);
		}
		else if (t.Type == -1)
		{
			r = eofHandler.Apply(t);
		}
		else
		{
			r = t;
		}

		return r;
	}

	public IDenterOptions GetOptions() => new DenterOptionsImpl(this);

	protected abstract IToken PullToken();

	private void InitIfFirstRun()
	{
		if (indentations.Count == 0)
		{
			indentations.Push(0);
			//indentations.AddFront(0);
			// First invocation. Look for the first non-NL. Enqueue it, and possibly an indentation if that non-NL
			// token doesn't start at char 0.
			IToken firstRealToken;
			do
			{
				firstRealToken = PullToken();
			} while (firstRealToken.Type == nlToken);

			if (firstRealToken.Column > 0)
			{
				indentations.Push(firstRealToken.Column);
				//indentations.AddFront(firstRealToken.Column);
				dentsBuffer.Enqueue(CreateToken(indentToken, firstRealToken));
			}

			dentsBuffer.Enqueue(firstRealToken);
		}
	}

	private IToken HandleNewlineToken(IToken t)
	{
		// fast-forward to the next non-NL
		var nextNext = PullToken();
		while (nextNext.Type == nlToken)
		{
			t = nextNext;
			nextNext = PullToken();
		}

		if (nextNext.Type == -1)
		{
			return eofHandler.Apply(nextNext);
		}
		// nextNext is now a non-NL token; we'll queue it up after any possible dents

		var nlText = t.Text;
		var indent = nlText.Length - 1; // every NL has one \n char, so shorten the length to account for it
		if (indent > 0 && nlText[0] == '\r')
		{
			--indent; // If the NL also has a \r char, we should account for that as well
		}

		var prevIndent = indentations.Peek();
		//int prevIndent = indentations.Get(0);
		IToken r;
		if (indent == prevIndent)
		{
			r = t; // just a newline
		}
		else if (indent > prevIndent)
		{
			r = CreateToken(indentToken, t);
			indentations.Push(indent);
			//indentations.AddFront(indent);
		}
		else
		{
			r = UnwindTo(indent, t);
		}

		dentsBuffer.Enqueue(nextNext);
		return r;
	}

	private IToken CreateToken(int tokenType, IToken copyFrom)
	{
		string? tokenTypeStr;
		if (tokenType == nlToken)
		{
			tokenTypeStr = "newline";
		}
		else if (tokenType == indentToken)
		{
			tokenTypeStr = "indent";
		}
		else if (tokenType == dedentToken)
		{
			tokenTypeStr = "dedent";
		}
		else
		{
			tokenTypeStr = null;
		}

		CommonToken r = new InjectedToken(copyFrom, tokenTypeStr);
		r.Type = tokenType;
		return r;
	}

	/**
	 * Returns a DEDENT token, and also queues up additional DEDENTS as necessary.
	 * @param targetIndent the "size" of the indentation (number of spaces) by the end
	 * @param copyFrom the triggering token
	 * @return a DEDENT token
	 */
	private IToken UnwindTo(int targetIndent, IToken copyFrom)
	{
		//assert _dentsBuffer.isEmpty() : _dentsBuffer;
		dentsBuffer.Enqueue(CreateToken(nlToken, copyFrom));
		// To make things easier, we'll queue up ALL of the dedents, and then pop off the first one.
		// For example, here's how some text is analyzed:
		//
		//  Text          :  Indentation  :  Action         : Indents Deque
		//  [ baseline ]  :  0            :  nothing        : [0]
		//  [   foo    ]  :  2            :  INDENT         : [0, 2]
		//  [    bar   ]  :  3            :  INDENT         : [0, 2, 3]
		//  [ baz      ]  :  0            :  DEDENT x2      : [0]

		while (true)
		{
			var prevIndent = indentations.Pop();
			//int prevIndent = indentations.RemoveFront();
			if (prevIndent == targetIndent)
			{
				break;
			}

			if (targetIndent > prevIndent)
			{
				// "weird" condition above
				indentations.Push(prevIndent);
				//indentations.AddFront(prevIndent); // restore previous indentation, since we've indented from it
				dentsBuffer.Enqueue(CreateToken(indentToken, copyFrom));
				break;
			}

			dentsBuffer.Enqueue(CreateToken(dedentToken, copyFrom));
		}

		indentations.Push(targetIndent);
		//indentations.AddFront(targetIndent);
		return dentsBuffer.Dequeue();
	}

	public static IBuilder0 Builder() => new BuilderImpl();

	private class StandardEofHandler : IEofHandler
	{
		private readonly DenterHelper helper;

		public StandardEofHandler(DenterHelper helper)
		{
			this.helper = helper;
		}

		public IToken Apply(IToken t)
		{
			IToken r;
			// when we reach EOF, unwind all indentations. If there aren't any, insert a NL. This lets the grammar treat
			// un-indented expressions as just being NL-terminated, rather than NL|EOF.
			if (helper.indentations.Count == 0)
			{
				r = helper.CreateToken(helper.nlToken, t);
				helper.dentsBuffer.Enqueue(t);
			}
			else
			{
				r = helper.UnwindTo(targetIndent: 0, t);
				helper.dentsBuffer.Enqueue(t);
			}

			helper.reachedEof = true;
			return r;
		}
	}

	private interface IEofHandler
	{
		IToken Apply(IToken t);
	}

	private class DenterOptionsImpl : IDenterOptions
	{
		private readonly DenterHelper helper;

		public DenterOptionsImpl(DenterHelper helper)
		{
			this.helper = helper;
		}

		public void IgnoreEof()
		{
			helper.eofHandler = new EofHandler(helper);
		}

		private class EofHandler : IEofHandler
		{
			private readonly DenterHelper helper;

			public EofHandler(DenterHelper helper)
			{
				this.helper = helper;
			}

			public IToken Apply(IToken t)
			{
				helper.reachedEof = true;
				return t;
			}
		}
	}

	private class InjectedToken : CommonToken
	{
		private readonly string? type;

		public InjectedToken(IToken oldToken, string? type) : base(oldToken)
		{
			this.type = type;
		}

		public string GetText()
		{
			if (type != null)
			{
				Text = type;
			}

			return Text;
		}
	}

	public interface IBuilder0
	{
		IBuilder1 Nl(int nl);
	}

	public interface IBuilder1
	{
		IBuilder2 Indent(int indent);
	}

	public interface IBuilder2
	{
		IBuilder3 Dedent(int dedent);
	}

	public interface IBuilder3
	{
		DenterHelper PullToken(Func<IToken> puller);
	}

	private class BuilderImpl : IBuilder0, IBuilder1, IBuilder2, IBuilder3
	{
		private int dedent;
		private int indent;
		private int nl;

		public IBuilder1 Nl(int nl)
		{
			this.nl = nl;
			return this;
		}

		public IBuilder2 Indent(int indent)
		{
			this.indent = indent;
			return this;
		}

		public IBuilder3 Dedent(int dedent)
		{
			this.dedent = dedent;
			return this;
		}

		public DenterHelper PullToken(Func<IToken> puller) => new DenterHelperImpl(nl, indent, dedent, puller);

		private class DenterHelperImpl : DenterHelper
		{
			private readonly Func<IToken> puller;

			public DenterHelperImpl(int nlToken, int indentToken, int dedentToken, Func<IToken> puller) : base(
				nlToken, indentToken, dedentToken)
			{
				this.puller = puller;
			}

			protected override IToken PullToken() => puller();
		}
	}
}

public interface IDenterOptions
{
	/**
	 * Don't do any special handling for EOFs; they'll just be passed through normally. That is, we won't unwind indents
	 * or add an extra NL.
	 * 
	 * This is useful when the lexer will be used to parse rules that are within a line, such as expressions. One use
	 * case for that might be unit tests that want to exercise these sort of "line fragments".
	 */
	void IgnoreEof();
}