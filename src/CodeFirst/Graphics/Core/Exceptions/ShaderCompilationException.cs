﻿namespace CodeFirst.Graphics.Core.Exceptions;

public class ShaderCompilationException : Exception
{
	public ShaderCompilationException(string? message) : base(message)
	{
	}
}