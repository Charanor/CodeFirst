using System;
using System.Collections.Generic;
using System.Linq;
using Ecs.Generators.Parsing;

namespace Ecs.Generators.Utils;

public class EcsDefinitionGenerator
{
	private const string CORE_NAMESPACE = "JXS.Ecs.Core";
	private const string ATTRIBUTE_NAMESPACE = $"{CORE_NAMESPACE}.Attributes";
	private const string ATTRIBUTE_GENERATION_NAMESPACE = $"{CORE_NAMESPACE}.Attributes.Generation";

	private const string COMPONENT_TYPE = "IComponent";
	private const string SINGLETON_COMPONENT_TYPE = "ISingletonComponent";
	private const string ITERATING_SYSTEM_TYPE = "IteratingSystem";
	private const string ORDERED_SYSTEM_TYPE = "OrderedIteratingSystem";

	private readonly EcsProgram program;
	private readonly ClassBuilder builder;

	private readonly HashSet<string> singletonComponentNames = new();

	public EcsDefinitionGenerator(EcsProgram program)
	{
		this.program = program;
		builder = new ClassBuilder();
	}

	public string GenerateSource()
	{
		UsingLine(CORE_NAMESPACE);
		UsingLine(ATTRIBUTE_NAMESPACE);
		UsingLine(ATTRIBUTE_GENERATION_NAMESPACE);

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
		AttributeLine(name: "ProcessPass", $"{CORE_NAMESPACE}.Pass.{system.ProcessPass.Pass.ToString()}");
		var components = system.Aspect.Components.ToList();
		var mandatory = components.Where(component => !component.IsOptional).ToList();
		if (mandatory.Count > 0)
		{
			var withoutSingletons = mandatory.Where(cmp => !singletonComponentNames.Contains(cmp.Name));
			AttributeLine(name: "All", string.Join(separator: ",", withoutSingletons.Select(c => $"typeof({c.Name})")));
		}

		using (system.IsOrdered ? OrderedSystemBlock(system.Name) : SystemBlock(system.Name))
		{
			foreach (var optionalComponent in components.Where(component => component.IsOptional))
			{
				// TODO: Generate optional utilities
			}
			
			// TODO: Generate Update method
			
			var visible = mandatory.Where(comp => !comp.IsTransient);
			builder.IndentedLn(
				$"private partial void ProcessEntity({string.Join(separator: ",", visible.Select(cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {cmp.Name} {ToCamelCase(cmp.Name)}"))});");
		}
	}

	private void UsingLine(string ns) => builder.IndentedLn($"using {ns};");

	private static string ToCamelCase(string input) => $"{char.ToLowerInvariant(input[0])}{input.Substring(1)}";

	private IDisposable ComponentBlock(string name) =>
		builder.Block($"public partial struct {name} : {COMPONENT_TYPE}");

	private IDisposable SingletonComponentBlock(string name) =>
		builder.Block($"public partial struct {name} : {SINGLETON_COMPONENT_TYPE}");

	private IDisposable SystemBlock(string name) =>
		builder.Block($"public partial class {name} : {ITERATING_SYSTEM_TYPE}");

	private IDisposable OrderedSystemBlock(string name) =>
		builder.Block($"public partial class {name} : {ORDERED_SYSTEM_TYPE}");

	private void AttributeLine(string name, string contents = "") =>
		builder.IndentedLn($"[{name}({contents})]");
}