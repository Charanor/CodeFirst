using System.Collections.Generic;

namespace CodeFirst.Generators.Graphics.Parsing;

public enum GLSLStorageType
{
	None,
	Input,
	Output,
	Uniform
}

public abstract record GLSLDefinition(ShaderType ParentShaderVariant);

public record GLSLShaderSource(ShaderType ParentShaderVariant, string Source) : GLSLDefinition(ParentShaderVariant);

public record GLSLVersion(ShaderType ParentShaderVariant, string Number, string? Profile) : GLSLDefinition(
	ParentShaderVariant)
{
	public bool HasProfile => Profile is { Length: > 0 };
}

public record GLSLExtension(ShaderType ParentShaderVariant, string Name, string Status) : GLSLDefinition(
	ParentShaderVariant);

public record GLSLDefine(ShaderType ParentShaderVariant, string Name, string Value) : GLSLDefinition(
	ParentShaderVariant);

public record GLSLMember
(ShaderType ParentShaderVariant, GLSLStorageType StorageType, string? Type,
	string? Identifier, int ArrayRanks, string[] ArraySizeGuesses, IEnumerable<string> GlslConstants) : GLSLDefinition(
	ParentShaderVariant)
{
	public bool HasConcreteType => Type is { Length: > 0 };
	public bool HasIdentifier => Identifier is { Length: > 0 };
}

public abstract record GLSLMemberGroup(ShaderType ParentShaderVariant, GLSLStorageType StorageType, string? Type,
	string? Identifier, IEnumerable<GLSLMember> Members, int ArrayRanks, string[] ArraySizeGuesses,
	IEnumerable<string> GlslConstants) :
	GLSLMember(ParentShaderVariant, StorageType, Type, Identifier, ArrayRanks, ArraySizeGuesses, GlslConstants);

public record GLSLInterface(ShaderType ParentShaderVariant, GLSLStorageType StorageType, string? Type,
	string? Identifier, IEnumerable<GLSLMember> Members, int ArrayRanks, string[] ArraySizeGuesses,
	IEnumerable<string> GlslConstants) :
	GLSLMemberGroup(ParentShaderVariant, StorageType, Type, Identifier, Members, ArrayRanks, ArraySizeGuesses,
		GlslConstants);

public record GLSLStruct(ShaderType ParentShaderVariant, GLSLStorageType StorageType, string? Type,
	string? Identifier, IEnumerable<GLSLMember> Members, int ArrayRanks, string[] ArraySizeGuesses,
	IEnumerable<string> GlslConstants) :
	GLSLMemberGroup(ParentShaderVariant, StorageType, Type, Identifier, Members, ArrayRanks, ArraySizeGuesses,
		GlslConstants);