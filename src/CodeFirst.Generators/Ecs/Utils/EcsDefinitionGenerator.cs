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
		if (system is { IsOrdered: true, PrimaryAspect.IsExternal: true })
		{
			// We can not have an ordered system with an external aspect
			// TODO: Exception
		}
		else if (system is { IsOrdered: true, IsAsync: true })
		{
			// We can not have an async ordered system, that makes no sense!
			// TODO: Exception
		}

		if (system.PrimaryAspect.IsExternal)
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
		using (SystemBlock(system.Name, "EntitySystem"))
		{
			var components = system.PrimaryAspect.Components.ToList();
			var mapperContexts = components.ToDictionary(
				cmp => cmp.Name,
				cmp => builder.ComponentMapper(
					cmp.Name,
					program.Namespace?.Name,
					IsSingleton(cmp),
					isIteratingSystem: false));

			var tagComponents = system.PrimaryAspect.Tags.ToList();
			var excludedComponents = system.PrimaryAspect.Excluded.ToList();
			AddComponentAttributes(components, tagComponents, excludedComponents);

			var identifier = system.PrimaryAspect.Identifier ?? "aspect";
			builder.IndentedLn(
				$"private readonly CodeFirst.Utils.Collections.IReadOnlySnapshotList<Entity> {identifier} = null!;");

			foreach (var aspect in system.ExternalAspects)
			{
				AddComponentAttributes(components, aspect.Tags.ToList(), aspect.Excluded.ToList());
				builder.IndentedLn(
					$"private readonly CodeFirst.Utils.Collections.IReadOnlySnapshotList<Entity> {aspect.Identifier} = null!;");
			}

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
		var primaryAspectComponents = system.PrimaryAspect.Components.ToList();
		var tagComponents = system.PrimaryAspect.Tags.ToList();
		var excludedComponents = system.PrimaryAspect.Excluded.ToList();
		AddComponentAttributes(primaryAspectComponents, tagComponents, excludedComponents);

		var systemClassName = system switch
		{
			{ IsOrdered: true } => "OrderedIteratingSystem",
			{ IsAsync: true } => "ParallelIteratingSystem",
			_ => "IteratingSystem"
		};
		using (SystemBlock(system.Name, systemClassName))
		{
			var allComponents = new List<EcsAspectComponent>();
			allComponents.AddRange(primaryAspectComponents);
			foreach (var externalComponent in system.ExternalAspects.SelectMany(a => a.Components))
			{
				var idx = allComponents.FindIndex(c => c.Name == externalComponent.Name);
				if (idx < 0)
				{
					allComponents.Add(externalComponent with { IsExternal = true });
					continue;
				}

				// Duplicate component definition, aggregate them
				var component = allComponents[idx];
				allComponents[idx] = component with
				{
					IsExternal = true,
					IsReadonly = component.IsReadonly && externalComponent.IsReadonly,
					IsOptional = component.IsOptional && externalComponent.IsOptional
				};
			}

			var mapperContexts = allComponents.ToDictionary(
				cmp => cmp.Name,
				cmp => builder.ComponentMapper(
					cmp.Name,
					program.Namespace?.Name,
					IsSingleton(cmp),
					isIteratingSystem: true));

			foreach (var aspect in system.ExternalAspects)
			{
				AddComponentAttributes(aspect.Components.ToList(), aspect.Tags.ToList(), aspect.Excluded.ToList());
				builder.IndentedLn(
					$"private readonly CodeFirst.Utils.Collections.IReadOnlySnapshotList<Entity> {aspect.Identifier} = null!;");
			}

			// This is a nice utility, but I'm afraid this will lead to developers doing something like:
			//		World.MySingleton.SomeProperty = "some value";
			// This could be an issue because it circumvents the safety measures that are in place for async processing.
			// For now, let's trust developers.
			builder.IndentedLn($"public new {WorldName} World {{ get; internal set; }}");

			foreach (var component in allComponents
				         .Where(component => component.IsOptional || component.IsExternal || IsSingleton(component)))
			{
				var mapperContext = mapperContexts[component.Name];
				builder.UtilityMethods(mapperContext, component.IsReadonly, component.IsOptional);
			}

			GenerateRemoveComponentMethods(allComponents, mapperContexts);

			// Generate current entity remove methods
			foreach (var component in primaryAspectComponents.Where(cmp => cmp is
				         { IsReadonly: false, IsOptional: false } && !IsSingleton(cmp)))
			{
				var mapperContext = mapperContexts[component.Name];
				using (builder.Block($"private void Remove(ref {component.Name} {ToCamelCase(component.Name)})"))
				{
					builder.FunctionCall("AssertHasEntity");
					builder.IndentedLn($"{mapperContext.FieldName}.Remove(CurrentEntity);");
				}
			}

			var paramComponents = primaryAspectComponents
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

				var paramList = paramComponents
					.Select(cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {ToCamelCase(cmp.Name)}")
					.ToList();
				if (paramList.Count > 0)
				{
					builder.FunctionCall("ProcessEntity", paramList);
					builder.FunctionCall("ProcessEntity", "delta", paramList);
				}
				else
				{
					builder.FunctionCall("ProcessEntity");
					builder.FunctionCall("ProcessEntity", "delta");
				}

				// We do not want to assign "CurrentEntity" for parallel iterating systems
				builder.AssignmentOp("CurrentEntity", "Entity.Invalid");
			}

			var processEntityParameterList = string.Join(",",
				paramComponents.Select(cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {cmp.Name} {ToCamelCase(cmp.Name)}"));
			if (processEntityParameterList.Length > 0)
			{
				builder.IndentedLn($"partial void ProcessEntity(float delta, {processEntityParameterList});");
				builder.IndentedLn($"partial void ProcessEntity({processEntityParameterList});");
			}
			else
			{
				builder.IndentedLn("partial void ProcessEntity(float delta);");
				builder.IndentedLn("partial void ProcessEntity();");
			}
		}
	}

	private void GenerateRemoveComponentMethods(IEnumerable<EcsAspectComponent> components,
		IReadOnlyDictionary<string, ComponentMapperContext> mapperContexts)
	{
		foreach (var component in components.Where(cmp =>
			         cmp is { IsReadonly: false, IsExternal: true } && !IsSingleton(cmp)))
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

				builder.IndentedLn("// This is to ensure components always get their ID:s in consistent order.");
				foreach (var (component, _) in program.Components)
				{
					builder.IndentedLn($"_ = ComponentManager.GetId<{component}>();");
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
								$"throw new System.NullReferenceException(\"Entity does not have {component}.\");");
						}

						builder.NewLine();
						builder.IndentedLn("return ref mapper.Get(entity);");
					}

					// Component checker
					using (builder.Block($"public bool Has{component}(Entity entity)"))
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
						builder.IndentedLn("return mapper.Has(entity);");
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

	private void AddComponentAttributes(List<EcsAspectComponent> components, List<EcsTagComponent> tags,
		List<EcsExcludedComponent> excluded)
	{
		var mandatory = components
			.Where(component => !component.IsOptional && !IsSingleton(component))
			.Cast<EcsAspectComponentBase>() // Cast instead of OfType because we want the exception fallback here
			.Concat(tags)
			.ToList();
		if (mandatory.Count > 0)
		{
			AttributeLine("All", string.Join(",", mandatory.Select(c => $"typeof({c.Name})")));
		}

		if (excluded.Count > 0)
		{
			AttributeLine("None", string.Join(",", excluded.Select(c => $"typeof({c.Name})")));
		}
	}

	private record EcsAspectComponentWithMetadata(
		string Name,
		bool IsOptional,
		bool IsReadonly,
		bool IsExternal,
		bool IsMultiAspect) : EcsAspectComponent(Name, IsOptional, IsReadonly, IsExternal);
}