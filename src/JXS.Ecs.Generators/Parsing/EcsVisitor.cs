using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecs.Generators.Parsing;

public class EcsVisitor : EcsBaseVisitor<IEnumerable<EcsDefinition>>
{
	private static IEnumerable<EcsDefinition> Empty => Enumerable.Empty<EcsDefinition>();

	protected override IEnumerable<EcsDefinition> DefaultResult => Empty;
	
	private static IEnumerable<EcsDefinition> Single(EcsDefinition definition) => new[] { definition };

	protected override IEnumerable<EcsDefinition> AggregateResult(IEnumerable<EcsDefinition> aggregate,
		IEnumerable<EcsDefinition> nextResult) => aggregate.Concat(nextResult);

	public override IEnumerable<EcsDefinition> VisitProgram(EcsParser.ProgramContext context)
	{
		var ns = context.@namespace()?.Accept(this).OfType<EcsNamespace>().FirstOrDefault();
		var components = context.components().Accept(this).OfType<EcsComponent>();
		var systems = context.systems().Accept(this).OfType<EcsSystem>();
		return Single(new EcsProgram(ns, components, systems));
	}

	public override IEnumerable<EcsDefinition> VisitNamespace(EcsParser.NamespaceContext context) =>
		Single(new EcsNamespace(context.NAMESPACE_IDENTIFIER().ToString()));

	public override IEnumerable<EcsDefinition> VisitComponent(EcsParser.ComponentContext context) =>
		Single(new EcsComponent(context.IDENTIFIER().ToString()));

	public override IEnumerable<EcsDefinition> VisitSystem(EcsParser.SystemContext context)
	{
		var name = context.IDENTIFIER().ToString();
		var processPass = context.processParam().Accept(this).OfType<EcsProcessPassParameter>().FirstOrDefault();
		if (processPass == null)
		{
			// TODO: Report error
			return Empty;
		}

		var aspect = context.aspectParam().Accept(this).OfType<EcsAspectParameter>().FirstOrDefault();
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (aspect == null)
		{
			// TODO: Report error
			return Empty;
		}

		return Single(new EcsSystem(name, processPass, aspect));
	}

	public override IEnumerable<EcsDefinition> VisitProcessParam(EcsParser.ProcessParamContext context)
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (!Enum.TryParse<ProcessPass>(context.PROCESS_PASS().ToString(), ignoreCase: false, out var pass))
		{
			// TODO: Report error
			return Empty;
		}

		return Single(new EcsProcessPassParameter(pass));
	}

	public override IEnumerable<EcsDefinition> VisitAspectParam(EcsParser.AspectParamContext context)
	{
		var aspectComponents = context.aspectComponent().SelectMany(cmp => cmp.Accept(this)).OfType<EcsAspectComponent>();
		return Single(new EcsAspectParameter(aspectComponents));
	}

	public override IEnumerable<EcsDefinition> VisitAspectComponent(EcsParser.AspectComponentContext context) => Single(
		new EcsAspectComponent(context.IDENTIFIER().ToString(), context.OPTIONAL() != null, context.READONLY() != null)
	);
}

public abstract record EcsDefinition;

public record EcsProgram(EcsNamespace? Namespace, IEnumerable<EcsComponent> Components,
	IEnumerable<EcsSystem> Systems) : EcsDefinition;

public record EcsNamespace(string Name) : EcsDefinition;

public record EcsComponent(string Name) : EcsDefinition;

public record EcsSystem(string Name, EcsProcessPassParameter ProcessPass, EcsAspectParameter Aspect) : EcsDefinition;

public record EcsProcessPassParameter(ProcessPass Pass) : EcsDefinition;

public record EcsAspectParameter(IEnumerable<EcsAspectComponent> Components) : EcsDefinition;

public record EcsAspectComponent(string Name, bool IsOptional, bool IsReadonly) : EcsDefinition;

public enum ProcessPass
{
	Update,
	Draw,
	FixedUpdate
}