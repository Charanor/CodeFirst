using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using JXS.Graphics.Generators.Parsing;

namespace JXS.Graphics.Generators.Utils;

public static class GLSLUtils
{
	private const string MATH_NAMESPACE = "OpenTK.Mathematics";
	private const string GRAPHICS_CORE_NAMESPACE = "JXS.Graphics.Core";
	private const string NON_BRACKETS = @"[^\[\[]";

	public static readonly ImmutableArray<string> Namespaces = new[]
	{
		MATH_NAMESPACE,
		GRAPHICS_CORE_NAMESPACE
	}.ToImmutableArray();

	public static string GLSLToCSArrayBrackets(string glslBrackets) =>
		Regex.Replace(glslBrackets, NON_BRACKETS, string.Empty);

	public static string FirstCharToUpper(string input) =>
		input switch
		{
			null => throw new ArgumentNullException(nameof(input)),
			"" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
			_ => input[0].ToString().ToUpper() + input.Substring(1)
		};

	public static string FirstCharToLower(string input) =>
		input switch
		{
			null => throw new ArgumentNullException(nameof(input)),
			"" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
			_ => input[0].ToString().ToLower() + input.Substring(1)
		};

	public static string GLSLTypeToOpenTKType(string glslType) => glslType switch
	{
		null => throw new ArgumentNullException(nameof(glslType)),
		"" => throw new ArgumentException($"{nameof(glslType)} cannot be empty", nameof(glslType)),
		_ when glslType.StartsWith("vec") => glslType.Replace(oldValue: "vec", newValue: "Vector"),
		_ when glslType.StartsWith("mat") => glslType.Replace(oldValue: "mat", newValue: "Matrix"),
		"sampler2D" => $"{GRAPHICS_CORE_NAMESPACE}.Texture2D",
		"samplerCube" => "int", // TODO: Replace "int" with "CubeTexture" or something
		"int" or "float" => glslType,
		_ => throw new ArgumentException($"Can not convert GLSL type {glslType} to OpenTK type", nameof(glslType))
	};

	public static GLSLStorageType StringToGLSLStorageType(string? str) => str?.ToLowerInvariant() switch
	{
		"in" => GLSLStorageType.Input,
		"out" => GLSLStorageType.Output,
		"uniform" => GLSLStorageType.Uniform,
		_ => GLSLStorageType.None
	};
}