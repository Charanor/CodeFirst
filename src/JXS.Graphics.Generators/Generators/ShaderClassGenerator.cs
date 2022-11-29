using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JXS.Graphics.Generators.Parsing;
using JXS.Graphics.Generators.Utils;

namespace JXS.Graphics.Generators.Generators;

public class ShaderClassGenerator
{
	private const string GENERATOR_VERSION = "1.0.0";

	private const string SHADER_PROGRAM_CLASS = "ShaderProgram";
	private const string MATERIAL_CLASS = "Material";
	private const string MATERIAL = "Material";

	private const string VERSION_TYPE = "(string VersionNumber, string VersionProfile)";
	private const string DEFAULT_GLSL_VERSION_PROFILE = "core";

	private const string EXTENSION_LIST_TYPE =
		"IEnumerable<(string Extension, string Behavior)>";

	private static readonly ImmutableArray<string> Namespaces = new[]
	{
		"JXS.Graphics.Core",
		"System.Collections.Generic",
		"OpenTK.Mathematics",
		"System.CodeDom.Compiler"
	}.Concat(GLSLUtils.Namespaces).Distinct().ToImmutableArray();

	private readonly string className;
	private readonly ImmutableArray<GLSLDefinition> definitions;

	public ShaderClassGenerator(string className, IEnumerable<GLSLDefinition> definitions)
	{
		this.className = className;
		this.definitions = definitions.ToImmutableArray();
	}

	public string GenerateClassSource()
	{
		var classBuilder = new ClassBuilder();
		BuildImports(classBuilder);
		BuildNamespace(classBuilder);
		BuildClass(classBuilder);
		return classBuilder.Generate();
	}

	private void BuildClass(ClassBuilder classBuilder)
	{
		classBuilder.IndentedLn(
			$@"[GeneratedCode({Quote("JXS.Graphics.Generators")}, {Quote(GENERATOR_VERSION)})]");
		classBuilder.BeginBlock($"public class {className} : {SHADER_PROGRAM_CLASS}");
		{
			BuildVersions(classBuilder);
			BuildExtensions(classBuilder);
			BuildSources(classBuilder);
			var uniformMembers = BuildUniformUtilities(classBuilder);
			BuildConstructor(classBuilder, uniformMembers);
			BuildVertexRecord(classBuilder);
			BuildStructs(classBuilder);
		}
		classBuilder.EndBlock();
	}

	private List<(string LocationName, string UniformName)> BuildUniformUtilities(ClassBuilder classBuilder)
	{
		var uniforms = GetDefinitions<GLSLMember>()
			.Where(member => member is { StorageType: GLSLStorageType.Uniform, Identifier: not null, Type: not null })
			.Distinct(GLSLMemberComparer.Instance);
		var uniformMembers = new List<(string LocationName, string UniformName)>();
		foreach (var uniform in uniforms)
		{
			GenerateForMember(uniform);
		}

		return uniformMembers;

		void GenerateForMember(GLSLMember member, string prefix = "")
		{
			var (_, _, type, identifier, arrayRanks) = member;
			if (type == null || identifier == null)
			{
				return;
			}

			if (member is GLSLMemberGroup memberGroup)
			{
				var (_, _, _, _, members, _) = memberGroup;
				foreach (var glslMember in members)
				{
					GenerateForMember(glslMember, $"{prefix}{identifier}.");
				}
			}
			else
			{
				var openTKType = GLSLUtils.GLSLTypeToOpenTKType(type);
				for (var i = 0; i < arrayRanks; i++)
				{
					openTKType += "[]";
				}

				// Fields
				var locationFieldName = LocationFieldName(identifier);
				uniformMembers.Add((LocationName: locationFieldName, UniformName: $"{prefix}{identifier}"));

				classBuilder.IndentedLn($"private readonly int {locationFieldName};"); // Location field
				classBuilder.IndentedLn($"private {openTKType} {identifier};"); // Cached value

				// Property
				classBuilder.BeginBlock($"public {openTKType} {PropertyName(identifier)}");
				{
					classBuilder.IndentedLn($"get => {identifier};");
					classBuilder.BeginBlock("set");
					{
						classBuilder.BeginBlock($"if ({identifier} == value)");
						{
							classBuilder.IndentedLn("return;");
						}
						classBuilder.EndBlock();
						classBuilder.IndentedLn($"{identifier} = value;");
						classBuilder.IndentedLn($"SetUniform({locationFieldName}, value);");
					}
					classBuilder.EndBlock();
				}
				classBuilder.EndBlock();
			}
		}

		string LocationFieldName(string glslIdentifier) => $"{glslIdentifier}LocationIdx";
		string PropertyName(string glslIdentifier) => GLSLUtils.FirstCharToUpper(glslIdentifier);
	}

	private void BuildStructs(ClassBuilder classBuilder)
	{
		var groups = GetDefinitions<GLSLMemberGroup>().Where(grp => grp is { Type: not null })
			.Distinct(GLSLMemberGroupComparer.Instance);
		foreach (var (_, _, type, identifier, members, _) in groups)
		{
			var csType = GLSLUtils.FirstCharToUpper(type!);
			if (csType == MATERIAL)
			{
				BuildMaterialRecord(classBuilder, members, identifier);
			}
			else
			{
				BuildMemberRecord(classBuilder, csType, members);
			}

			classBuilder.NewLine();
		}
	}

	private static void BuildMemberRecord(ClassBuilder classBuilder, string csType, IEnumerable<GLSLMember> members)
	{
		classBuilder.IndentedLn($"public readonly record struct {csType}({ParamList(members)});");
	}

	private void BuildMaterialRecord(ClassBuilder classBuilder, IEnumerable<GLSLMember> members, string? identifier)
	{
		classBuilder.BeginBlock($"public record {MATERIAL} : {MATERIAL_CLASS}(new {className}())");
		{
			foreach (var (_, _, childType, childIdentifier, arrayRanks) in members.Where(member =>
				         member is { HasIdentifier: true, HasConcreteType: true }))
			{
				if (childType == null || childIdentifier == null)
				{
					continue;
				}

				var csChildType = BuildBackingField(classBuilder, childType, childIdentifier, arrayRanks,
					out var fieldName);
				BuildProperty(classBuilder, identifier, csChildType, arrayRanks, fieldName, childIdentifier);
			}
		}
		classBuilder.EndBlock();
	}

	private static void BuildProperty(ClassBuilder classBuilder, string? identifier, string csChildType, int arrayRanks,
		string fieldName, string childIdentifier)
	{
		var propertyName = GLSLUtils.FirstCharToUpper(fieldName);
		classBuilder.BeginBlock($"public {Array(csChildType, arrayRanks)} {propertyName}");
		{
			classBuilder.IndentedLn($"get => {fieldName};");
			classBuilder.BeginBlock("init");
			{
				classBuilder.IndentedLn($"{propertyName} = value;");
				var compoundName = Quote(identifier is { Length: > 0 }
					? $"{identifier}.{childIdentifier}"
					: childIdentifier);
				var arraySuffix =
					string.Concat(Enumerable.Repeat(element: "Array", arrayRanks));
				classBuilder.IndentedLn(
					$"Set{GLSLUtils.FirstCharToUpper(csChildType)}{arraySuffix}({compoundName}, {fieldName});");
			}
			classBuilder.EndBlock();
		}
		classBuilder.EndBlock();
	}

	private static string BuildBackingField(ClassBuilder classBuilder, string childType, string childIdentifier,
		int arrayRanks, out string fieldName)
	{
		var csChildType = GLSLUtils.GLSLTypeToOpenTKType(childType);
		fieldName = GLSLUtils.FirstCharToLower(childIdentifier);
		classBuilder.IndentedLn(
			$"private readonly {Array(csChildType, arrayRanks)} {fieldName};");
		return csChildType;
	}

	private void BuildVertexRecord(ClassBuilder classBuilder)
	{
		var inputs = GetDefinitions<GLSLMember>().Where(def => def is
			{ ParentShaderVariant: ShaderType.Vertex, StorageType: GLSLStorageType.Input });
		classBuilder.IndentedLn($"public readonly record struct Vertex({ParamList(inputs)});");
		classBuilder.NewLine();
	}

	private void BuildConstructor(ClassBuilder classBuilder,
		IEnumerable<(string LocationName, string UniformName)> uniformMembers)
	{
		classBuilder.BeginBlock(
			$"public {className}() : base({ShaderType.Vertex.GetSourceConstantName()}, {ShaderType.Fragment.GetSourceConstantName()})");
		{
			foreach (var (locationName, uniformName) in uniformMembers)
			{
				classBuilder.IndentedLn($"{locationName} = GetUniformLocation({Quote(uniformName)});");
			}
		}
		classBuilder.EndBlock();
		classBuilder.NewLine();
	}

	private void BuildSources(ClassBuilder classBuilder)
	{
		foreach (var (shaderType, source) in GetDefinitions<GLSLShaderSource>())
		{
			classBuilder.IndentedLn(
				$@"public static readonly string {shaderType.GetSourceConstantName()} = @{Quote(source)};");
		}

		classBuilder.NewLine();
	}

	private void BuildExtensions(ClassBuilder classBuilder)
	{
		var shaderGroupedExtensions = GetDefinitions<GLSLExtension>().GroupBy(ext => ext.ParentShaderVariant);
		foreach (var glslExtensions in shaderGroupedExtensions)
		{
			var extName = glslExtensions.Key.GetName();
			classBuilder.BeginBlock(
				$"public static readonly {EXTENSION_LIST_TYPE} {extName}Extensions => new []");
			{
				foreach (var (_, name, status) in glslExtensions)
				{
					classBuilder.IndentedLn($"({Quote(name)}, {Quote(status)}),");
				}
			}
			classBuilder.EndBlock(";");
		}

		classBuilder.NewLine();
	}

	private void BuildVersions(ClassBuilder classBuilder)
	{
		foreach (var (shaderType, number, profile) in GetDefinitions<GLSLVersion>())
		{
			var versionType = CreateVersionType(number, profile ?? DEFAULT_GLSL_VERSION_PROFILE);
			classBuilder.IndentedLn(
				@$"public static readonly {VERSION_TYPE} {shaderType.GetSourceConstantName()}Version = {versionType};");
		}

		classBuilder.NewLine();
	}

	private static void BuildNamespace(ClassBuilder classBuilder)
	{
		classBuilder.IndentedLn("namespace JXS.Graphics.Generated;");
		classBuilder.NewLine();
	}

	private static void BuildImports(ClassBuilder classBuilder)
	{
		foreach (var ns in Namespaces)
		{
			classBuilder.IndentedLn($"using {ns};");
		}

		classBuilder.NewLine();
	}

	// False-positive, OfType<T> IS a pure function
	// ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
	private IEnumerable<T> GetDefinitions<T>() where T : GLSLDefinition => definitions.OfType<T>();

	private static string ParamList(IEnumerable<GLSLMember> members)
	{
		var inputs = members.Where(def => def is { HasConcreteType: true, HasIdentifier: true });
		return string.Join(separator: ", ", inputs.Select(Parameter));

		// Functions
		string Parameter(GLSLMember member) =>
			$"{Array(GLSLUtils.GLSLTypeToOpenTKType(member.Type!), member.ArrayRanks)} {GLSLUtils.FirstCharToUpper(member.Identifier!)}";
	}

	private static string Array(string type, int ranks) =>
		$"{type}{string.Concat(Enumerable.Repeat(element: "[]", ranks))}";

	private static string CreateVersionType(string versionNumber, string versionProfile) =>
		$"(VersionNumber: {Quote(versionNumber)}, VersionProfile: {Quote(versionProfile)})";

	private static string Quote(string str) => $@"""{str}""";

	private class GLSLMemberGroupComparer : IEqualityComparer<GLSLMemberGroup>
	{
		public static readonly GLSLMemberGroupComparer Instance = new();

		public bool Equals(GLSLMemberGroup x, GLSLMemberGroup y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (ReferenceEquals(x, objB: null))
			{
				return false;
			}

			if (ReferenceEquals(y, objB: null))
			{
				return false;
			}

			if (x.GetType() != y.GetType())
			{
				return false;
			}

			if (x.Type == null || y.Type == null)
			{
				return false;
			}

			return x.Type == y.Type;
		}

		public int GetHashCode(GLSLMemberGroup obj) => obj.Type != null ? obj.Type.GetHashCode() : 0;
	}

	private class GLSLMemberComparer : IEqualityComparer<GLSLMember>
	{
		public static readonly GLSLMemberComparer Instance = new();


		public bool Equals(GLSLMember x, GLSLMember y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (ReferenceEquals(x, objB: null))
			{
				return false;
			}

			if (ReferenceEquals(y, objB: null))
			{
				return false;
			}

			if (x.GetType() != y.GetType())
			{
				return false;
			}

			return x.Type == y.Type && x.Identifier == y.Identifier;
		}

		public int GetHashCode(GLSLMember obj)
		{
			unchecked
			{
				return ((obj.Type != null ? obj.Type.GetHashCode() : 0) * 397) ^
				       (obj.Identifier != null ? obj.Identifier.GetHashCode() : 0);
			}
		}
	}
}