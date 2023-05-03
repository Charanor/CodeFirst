using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecs.Generators.Parsing;

public class EcsVisitor : EcsBaseVisitor<IEnumerable<EcsDefinition>>
{
	private static IEnumerable<EcsDefinition> Empty => Enumerable.Empty<EcsDefinition>();

	protected override IEnumerable<EcsDefinition> DefaultResult => Empty;

	private static IEnumerable<T> Single<T>(T definition) where T : EcsDefinition => new[] { definition };

	protected override IEnumerable<EcsDefinition> AggregateResult(IEnumerable<EcsDefinition> aggregate,
		IEnumerable<EcsDefinition> nextResult) => aggregate.Concat(nextResult);

	public override IEnumerable<EcsDefinition> VisitProgram(EcsParser.ProgramContext context)
	{
		var ns = context.@namespace()?.Accept(this).OfType<EcsNamespace>().FirstOrDefault();
		var components = context.components().Accept(this).OfType<EcsComponent>();
		var systems = context.systems().Accept(this).OfType<EcsSystem>();
		var world = context.world().Accept(this).OfType<EcsWorld>().First();
		return Single(new EcsProgram(ns, components, systems, world));
	}

	public override IEnumerable<EcsDefinition> VisitNamespace(EcsParser.NamespaceContext context) =>
		Single(new EcsNamespace(context.NamespaceIdentifier().ToString()));

	public override IEnumerable<EcsDefinition> VisitComponent(EcsParser.ComponentContext context) =>
		Single(new EcsComponent(context.Identifier().ToString(), context.SINGLETON() != null));

	public override IEnumerable<EcsDefinition> VisitSystem(EcsParser.SystemContext context)
	{
		var name = context.Identifier().ToString();
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

		return Single(new EcsSystem(name, processPass, aspect, context.ORDERED() != null));
	}

	public override IEnumerable<EcsDefinition> VisitProcessParam(EcsParser.ProcessParamContext context)
	{
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (!Enum.TryParse<ProcessPass>(context.ProcessPass().ToString(), ignoreCase: false, out var pass))
		{
			// TODO: Report error
			return Empty;
		}

		return Single(new EcsProcessPassParameter(pass));
	}

	public override IEnumerable<EcsDefinition> VisitAspectParam(EcsParser.AspectParamContext context)
	{
		var aspectComponents =
			context.aspectComponent().SelectMany(cmp => cmp.Accept(this)).OfType<EcsAspectComponent>();
		return Single(new EcsAspectParameter(aspectComponents, context.EXTERNAL() != null));
	}

	public override IEnumerable<EcsDefinition> VisitTransientComponent(EcsParser.TransientComponentContext context) =>
		Single(new EcsAspectComponent(context.Identifier().ToString(),
			IsTransient: true,
			IsOptional: false, // Transients are never optional
			IsReadonly: true, // Transients are always readonly
			IsExternal: false // Transients are never external
		));

	public override IEnumerable<EcsDefinition> VisitNonTransientComponent(
		EcsParser.NonTransientComponentContext context)
		=> Single(new EcsAspectComponent(
			context.Identifier().ToString(),
			IsTransient: false,
			context.OPTIONAL() != null,
			context.READONLY() != null,
			context.EXTERNAL() != null
		));

	public override IEnumerable<EcsDefinition> VisitWorld(EcsParser.WorldContext context) => Single(new EcsWorld(
		context.Identifier()?.ToString(),
		(
			context.worldBody().PreWildcard?.Accept(this).OfType<EcsWorldSystemCollection>() ??
			Enumerable.Empty<EcsWorldSystemCollection>()
		).ToList(),
		(
			context.worldBody().PostWildcard?.Accept(this).OfType<EcsWorldSystemCollection>() ??
			Enumerable.Empty<EcsWorldSystemCollection>()
		).ToList()
	));

	public override IEnumerable<EcsDefinition> VisitWorldSystemDeclaration(
		EcsParser.WorldSystemDeclarationContext context)
	{
		var systems = context.worldSystem() != null
			? context.worldSystem().Accept(this).OfType<EcsWorldSystem>()
			: context.worldSystemList().Accept(this).OfType<EcsWorldSystem>();
		return Single(new EcsWorldSystemCollection(systems.ToList()));
	}

	public override IEnumerable<EcsDefinition> VisitWorldSystem(EcsParser.WorldSystemContext context) =>
		Single(new EcsWorldSystem(context.Identifier().ToString()));
}

public abstract record EcsDefinition;

public record EcsProgram(EcsNamespace? Namespace, IEnumerable<EcsComponent> Components,
	IEnumerable<EcsSystem> Systems, EcsWorld World) : EcsDefinition;

public record EcsNamespace(string Name) : EcsDefinition;

public record EcsComponent(string Name, bool IsSingleton) : EcsDefinition;

public record EcsSystem
	(string Name, EcsProcessPassParameter ProcessPass, EcsAspectParameter Aspect, bool IsOrdered) : EcsDefinition;

public record EcsProcessPassParameter(ProcessPass Pass) : EcsDefinition;

public enum ProcessPass
{
	Update,
	Draw,
	FixedUpdate
}

public record EcsAspectParameter(IEnumerable<EcsAspectComponent> Components, bool IsExternal) : EcsDefinition;

public record EcsAspectComponent(string Name, bool IsTransient, bool IsOptional, bool IsReadonly, bool IsExternal) : EcsDefinition;

public record EcsWorld(
	string? Name,
	List<EcsWorldSystemCollection> PreWildcardSystems,
	List<EcsWorldSystemCollection> PostWildcardSystems
) : EcsDefinition;

public record EcsWorldSystemCollection(List<EcsWorldSystem> Systems) : EcsDefinition;

public record EcsWorldSystem(string Name) : EcsDefinition;