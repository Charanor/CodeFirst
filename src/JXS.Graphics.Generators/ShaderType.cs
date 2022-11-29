using System;

namespace JXS.Graphics.Generators;

public enum ShaderType
{
	Unknown,
	Vertex,
	Fragment
}

public static class ShaderTypeExtensions
{
	public static string GetSourceConstantName(this ShaderType @this) => $"{@this.GetName()}Source";
	public static string GetName(this ShaderType @this) => Enum.GetName(typeof(ShaderType), @this) ?? "UNKNOWN";
}