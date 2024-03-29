﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CodeFirst.Generators.Graphics.Parsing;

namespace CodeFirst.Generators.Graphics.Utils;

public static class GLSLUtils
{
	private const string MATH_NAMESPACE = "OpenTK.Mathematics";
	private const string GRAPHICS_CORE_NAMESPACE = "CodeFirst.Graphics.Core";
	private const string NON_BRACKETS = @"[^\[\]]";
	private const string BRACKETS = @"[\[\]]";

	public static readonly ImmutableArray<string> Namespaces = new[]
	{
		MATH_NAMESPACE,
		GRAPHICS_CORE_NAMESPACE
	}.ToImmutableArray();

	public static string GLSLToCSArrayBrackets(string glslBrackets) =>
		Regex.Replace(glslBrackets, NON_BRACKETS, string.Empty);

	public static IEnumerable<string> GLSLArraySizeConstants(string glslBrackets) => glslBrackets
		.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries)
		.Select(str => Regex.Replace(str, BRACKETS, string.Empty));

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
		"int" or "float" or "bool" => glslType,
		_ when glslType.StartsWith("vec") => glslType.Replace("vec", "Vector"),
		_ when glslType.StartsWith("mat") => glslType.Replace("mat", "Matrix"),
		"sampler2D" => $"{GRAPHICS_CORE_NAMESPACE}.Texture2D",
		"samplerCube" => "int", // TODO: Replace "int" with "CubeTexture" or something
		_ => throw new ArgumentException($"Can not convert GLSL type {glslType} to OpenTK type", nameof(glslType))
	};

	public static GLSLStorageType StringToGLSLStorageType(string? str) => str?.ToLowerInvariant() switch
	{
		"in" => GLSLStorageType.Input,
		"out" => GLSLStorageType.Output,
		"uniform" => GLSLStorageType.Uniform,
		_ => GLSLStorageType.None
	};

	public static bool IsGLSLConstant(string glslConstantName) => glslConstantName.StartsWith("gl_");

	public static string GetGLSLConstantFieldName(string glslConstantName) =>
		FirstCharToLower(GetGLConstantForGLSLConstant(glslConstantName));

	public static string GetGLFunctionForGLSLConstant(string glslConstantName, string refVariableName) =>
		$"GL.GetInteger(GetPName.{GetGLConstantForGLSLConstant(glslConstantName)}, ref {refVariableName});";

	private static string GetGLConstantForGLSLConstant(string glslConstantName) =>
		glslConstantName.Replace("gl_", "");
}