using System;
using System.Linq;
using Ecs.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace Ecs.Generators.Utils;

public class EcsDefinitionGenerator
{
	private const string CORE_NAMESPACE = "JXS.Ecs.Core";
	private const string COMPONENT_TYPE = $"{CORE_NAMESPACE}.IComponent";
	private const string SINGLETON_COMPONENT_METADATA_NAME = $"{CORE_NAMESPACE}.ISingletonComponent";
	private const string ITERATING_SYSTEM_TYPE = $"{CORE_NAMESPACE}.IteratingSystem";

	private const string ATTRIBUTE_NAMESPACE = $"{CORE_NAMESPACE}.Attributes";

	private readonly EcsProgram program;
	private readonly Compilation compilation;
	private readonly ClassBuilder builder;

	private readonly INamedTypeSymbol componentType;
	private readonly INamedTypeSymbol singletonComponentType;
	private readonly INamedTypeSymbol iteratingSystemType;

	public EcsDefinitionGenerator(EcsProgram program, Compilation compilation)
	{
		this.program = program;
		this.compilation = compilation;
		builder = new ClassBuilder();

		componentType = compilation.GetTypeByMetadataName(COMPONENT_TYPE)!;
		singletonComponentType = compilation.GetTypeByMetadataName(SINGLETON_COMPONENT_METADATA_NAME)!;
		iteratingSystemType = compilation.GetTypeByMetadataName(ITERATING_SYSTEM_TYPE)!;
	}

	public string GenerateSource()
	{
		if (program.Namespace != null)
		{
			builder.IndentedLn($"namespace {program.Namespace.Name};");
		}

		foreach (var component in program.Components)
		{
			GenerateComponent(component);
		}

		foreach (var system in program.Systems)
		{
			GenerateSystem(system);
		}

		return builder.Generate();
	}

	private void GenerateComponent(EcsComponent component)
	{
		using (ComponentBlock(component.Name))
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
			AttributeLine(name: "All", string.Join(separator: ",", mandatory.Select(c => $"typeof({c.Name})")));
		}

		var optional = components.Where(component => component.IsOptional).ToList();
		if (optional.Count > 0)
		{
			GenerationAttributeLine(name: "GenerateComponentUtilities",
				string.Join(separator: ",", optional.Select(c => $"typeof({c.Name})")));
		}

		using (SystemBlock(system.Name))
		{
			// No contents for now
			GenerationAttributeLine("EntityProcessor");
			builder.IndentedLn(
				$"private partial void ProcessEntity({string.Join(separator: ",", mandatory.Select(cmp => $"{(cmp.IsReadonly ? "in" : "ref")} {cmp.Name} {char.ToLowerInvariant(cmp.Name[0])}{cmp.Name.Substring(1)}"))});");
		}
	}

	private IDisposable ComponentBlock(string name) =>
		builder.Block($"public partial struct {name} : {COMPONENT_TYPE}");

	private IDisposable SystemBlock(string name) =>
		builder.Block($"public partial class {name} : {ITERATING_SYSTEM_TYPE}");

	private void AttributeLine(string name, string contents = "") =>
		builder.IndentedLn($"[{ATTRIBUTE_NAMESPACE}.{name}({contents})]");

	private void GenerationAttributeLine(string name, string contents = "") =>
		builder.IndentedLn($"[{ATTRIBUTE_NAMESPACE}.Generation.{name}({contents})]");
}