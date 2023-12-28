using System;
using System.Collections.Generic;
using System.Linq;
using CodeFirst.Generators.Ecs.Parsing;

namespace CodeFirst.Generators.Ecs.Utils;

public class EcsDefinitionGenerator
{
	private const string CORE_NAMESPACE = "CodeFirst.Ecs.Core";
	private const string ATTRIBUTE_NAMESPACE = $"{CORE_NAMESPACE}.Attributes";
	private const string ATTRIBUTE_GENERATION_NAMESPACE = $"{CORE_NAMESPACE}.Attributes.Generation";
	private const string EXCEPTIONS_NAMESPACE = $"{CORE_NAMESPACE}.Exceptions";

	private const string COMPONENT_TYPE = "IComponent";
	private const string SINGLETON_COMPONENT_TYPE = "ISingletonComponent";

	private readonly EcsProgram program;
	private readonly ClassBuilder builder;

	private readonly HashSet<string> singletonComponentNames = new();

	public EcsDefinitionGenerator(EcsProgram program)
	{
		this.program = program;
		builder = new ClassBuilder();
	}

	private string WorldName => program.World.Name ?? "GeneratedWorld";

	public string GenerateSource()
	{
		UsingLine(CORE_NAMESPACE);
		UsingLine(ATTRIBUTE_NAMESPACE);
		UsingLine(ATTRIBUTE_GENERATION_NAMESPACE);
		UsingLine(EXCEPTIONS_NAMESPACE);

		if (program.Namespace != null)
		{
			builder.IndentedLn($"namespace {program.Namespace.Name};");
		}

		foreach (var component in program.Components)
		{
			GenerateComponent(component);
			if (component.IsSingleton)
			{
				singletonComponentNames.Add(component.Name);
			}
		}

		foreach (var system in program.Systems)
		{
			GenerateSystem(system);
		}

		GenerateWorld();

		return builder.Generate();
	}

	private void GenerateComponent(EcsComponent component)
	{
		using (component.IsSingleton ? SingletonComponentBlock(component.Name) : ComponentBlock(component.Name))
		{
			// No contents for now
		}
	}

	private void GenerateSystem(EcsSystem system)
	{
		AttributeLine("ProcessPass", $"{CORE_NAMESPACE}.Pass.{system.ProcessPass.Pass.ToString()}");

		// TODO: Change this logic when we implement multiple aspects
		if (system is { IsOrdered: true, Aspect.IsExternal: true })
		{
			// We can not have an ordered system with an external aspect
			// TODO: Exception
		}

		if (system.Aspect.IsExternal)
		{
			GenerateExternalAspectSystem(system);
		}
		else
		{
			GenerateDefaultSystem(system);
		}
	}

	private void GenerateExternalAspectSystem(EcsSystem system)
	{
		var components = system.Aspect.Components.ToList();
		using (SystemBlock(system.Name, "EntitySystem"))
		{
			var mapperContexts = components.ToDictionary(
				cmp => cmp.Name,
				cmp => builder.ComponentMapper(
					cmp.Name,
					program.Namespace?.Name,
					IsSingleton(cmp),
					isIteratingSystem: false));

			builder.IndentedLn($"public new {WorldName} World {{ get; internal set; }}");

			foreach (var component in components)
			{
				var mapperContext = mapperContexts[component.Name];
				builder.UtilityMethods(mapperContext, component.IsReadonly, component.IsOptional);
			}

			GenerateRemoveComponentMethods(components, mapperContexts);
		}
	}

	private void GenerateDefaultSystem(EcsSystem system)
	{
		var components = system.Aspect.Components.ToList();
		var mandatory = components
			.Where(component => !component.IsOptional && !IsSingleton(component))
			.Cast<EcsAspectComponentBase>()
			.Concat(system.Aspect.Tags)
			.ToList();
		if (mandatory.Count > 0)
		{
			AttributeLine("All", string.Join(",", mandatory.Select(c => $"typeof({c.Name})")));
		}

		var excluded = system.Aspect.Excluded.ToList();
		if (excluded.Count > 0)
		{
			AttributeLine("None", string.Join(",", excluded.Select(c => $"typeof({c.Name})")));
		}

		using (SystemBlock(system.Name, system.IsOrdered ? "OrderedIteratingSystem" : "IteratingSystem"))
		{
			var mapperContexts = components.ToDictionary(
				cmp => cmp.Name,
				cmp => builder.ComponentMapper(
					cmp.Name,
					program.Namespace?.Name,
					IsSingleton(cmp),
					isIteratingSystem: true));

			// This is a nice utility, but I'm afraid this will lead to developers doing something like:
			//		World.MySingleton.SomeProperty = "some value";
			// This could be an issue because it circumvents the safety measures that are in place for async processing.
			// For now, let's trust developers.
			builder.IndentedLn($"public new {WorldName} World {{ get; internal set; }}");

			foreach (var component in components.Where(component =>
				         component.IsOptional ||
				         component.IsExternal ||
				         IsSingleton(component)))
			{
				var mapperContext = mapperContexts[component.Name];
				builder.UtilityMethods(mapperContext, component.IsReadonly, component.IsOptional);
			}

			GenerateRemoveComponentMethods(components, mapperContexts);

			// Generate current entity remove methods
			foreach (var component in components.Where(cmp => cmp is
				         { IsReadonly: false, IsOptional: false } && !IsSingleton(cmp)))
			{
				var mapperContext = mapperContexts[component.Name];
				using (builder.Block($"private void Remove(ref {component.Name} {ToCamelCase(component.Name)})"))
				{
					builder.FunctionCall("AssertHasEntity", "nameof(Remove)");
					builder.IndentedLn($"{mapperContext.FieldName}.Remove(CurrentEntity);");
				}
			}

			var paramComponents = components
				.Where(cmp => cmp is { IsOptional: false } && !IsSingleton(cmp))
				.ToList();
			using (builder.Block("protected override void Update(Entity entity, float delta)"))
			{
				builder.AssignmentOp("CurrentEntity", "entity");
				foreach (var component in paramComponents)
				{
					var mapperContext = mapperContexts[component.Name];
					var varModifier = component.IsReadonly ? "ref readonly" : "ref";
					builder.IndentedLn(
						$"{varModifier} {component.Name} {ToCamelCase(component.Name)} = ref {mapperContext.FieldName}.Get(entity);");
				}

				var paramList = paramComponents.Select(
					cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {ToCamelCase(cmp.Name)}"
				).ToList();
				builder.FunctionCall("ProcessEntity", paramList);
				builder.FunctionCall("ProcessEntity", "delta", paramList);
				builder.AssignmentOp("CurrentEntity", "Entity.Invalid");
			}

			var processEntityParameterList = string.Join(",",
				paramComponents.Select(cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {cmp.Name} {ToCamelCase(cmp.Name)}"));
			builder.IndentedLn($"partial void ProcessEntity(float delta, {processEntityParameterList});");
			builder.IndentedLn($"partial void ProcessEntity({processEntityParameterList});");
		}
	}

	private void GenerateRemoveComponentMethods(IEnumerable<EcsAspectComponent> components,
		IReadOnlyDictionary<string, ComponentMapperContext> mapperContexts)
	{
		foreach (var component in components.Where(cmp =>
			         cmp is { IsReadonly: false } && !IsSingleton(cmp)))
		{
			var mapperContext = mapperContexts[component.Name];
			using (builder.Block(
				       $"private void Remove(Entity entity, ref {component.Name} {ToCamelCase(component.Name)})"))
			{
				builder.IndentedLn($"{mapperContext.FieldName}.Remove(entity);");
			}
		}
	}

	private void GenerateWorld()
	{
		var world = program.World;
		using (builder.Block($"public partial class {WorldName} : World"))
		{
			// Constructor, add systems
			using (builder.Block($"public {WorldName}() : base()"))
			{
				var preWildcardSystemNames = world.PreWildcardSystems
					.SelectMany(group => group.Systems)
					.Select(system => system.Name)
					.ToList();

				var postWildcardSystemNames = world.PostWildcardSystems
					.SelectMany(group => group.Systems)
					.Select(system => system.Name)
					.ToList();

				foreach (var system in preWildcardSystemNames)
				{
					AddSystem(system);
				}

				var wildcardSystemNames = program.Systems
					.Where(system =>
						!preWildcardSystemNames.Contains(system.Name) &&
						!postWildcardSystemNames.Contains(system.Name))
					.Select(system => system.Name);
				foreach (var system in wildcardSystemNames)
				{
					AddSystem(system);
				}

				foreach (var system in postWildcardSystemNames)
				{
					AddSystem(system);
				}

				void AddSystem(string system)
				{
					builder.IndentedLn($"var {system}Instance = new {system} {{ World = this }};");
					builder.IndentedLn($"AddSystem({system}Instance);");
				}
			}

			foreach (var (component, isSingleton) in program.Components)
			{
				if (isSingleton)
				{
					// Singleton component accessors
					builder.IndentedLn(
						$"public ref {component} {component} => ref GetSingletonComponent<{component}>();");
				}
				else
				{
					// Component accessor
					using (builder.Block($"public ref {component} Get{component}(Entity entity)"))
					{
						using (builder.Block("if (!entity.IsValid)"))
						{
							builder.IndentedLn("throw new InvalidEntityException();");
						}

						using (builder.Block("if (!HasEntity(entity))"))
						{
							builder.IndentedLn("throw new EntityDoesNotExistException(entity);");
						}

						builder.NewLine();
						builder.IndentedLn($"var mapper = GetMapper<{component}>();");
						using (builder.Block("if (!mapper.Has(entity))"))
						{
							builder.IndentedLn(
								$"throw new NullReferenceException(\"Entity does not have {component}.\");");
						}

						builder.NewLine();
						builder.IndentedLn("return ref mapper.Get(entity);");
					}
				}
			}
		}
	}

	private void UsingLine(string ns) => builder.IndentedLn($"using {ns};");

	private static string ToCamelCase(string input) => $"{char.ToLowerInvariant(input[0])}{input.Substring(1)}";

	private IDisposable ComponentBlock(string name) =>
		builder.Block($"public partial struct {name} : {COMPONENT_TYPE}");

	private IDisposable SingletonComponentBlock(string name) =>
		builder.Block($"public partial struct {name} : {SINGLETON_COMPONENT_TYPE}");

	private IDisposable SystemBlock(string name, string baseClass) =>
		builder.Block($"public partial class {name} : {baseClass}");

	private void AttributeLine(string name, string contents = "") =>
		builder.IndentedLn($"[{name}({contents})]");

	private bool IsSingleton(EcsAspectComponent component) => singletonComponentNames.Contains(component.Name);
}