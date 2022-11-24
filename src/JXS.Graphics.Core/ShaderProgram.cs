using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using JXS.Graphics.Core.Exceptions;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public class ShaderProgram : NativeResource
{
	// ReSharper disable once InconsistentNaming
	protected const int GL_FALSE = 0;

	private readonly ProgramHandle handle;

	private readonly IImmutableDictionary<string, ActiveUniformInfo> uniformNameMapper;
	private readonly IImmutableDictionary<int, ActiveUniformInfo> uniformLocationMapper;

	public ShaderProgram(string vertexShaderSource, string fragmentShaderSource)
	{
		handle = CreateProgram();

		var vertexShaderId = CompileShader(vertexShaderSource, ShaderType.VertexShader);
		AttachShader(handle, vertexShaderId);

		var fragmentShaderId = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);
		AttachShader(handle, fragmentShaderId);

		LinkProgram(handle);
		var success = 0;
		GetProgrami(handle, ProgramPropertyARB.LinkStatus, ref success);
		if (success == GL_FALSE)
		{
			GetProgramInfoLog(handle, out var infoLog);
			throw new ShaderCompilationException(infoLog);
		}

		DetachShader(handle, vertexShaderId);
		DeleteShader(vertexShaderId);

		DetachShader(handle, fragmentShaderId);
		DeleteShader(fragmentShaderId);

		(uniformNameMapper, uniformLocationMapper) = CreateUniformMappers();
	}

	private (IImmutableDictionary<string, ActiveUniformInfo>, IImmutableDictionary<int, ActiveUniformInfo>)
		CreateUniformMappers()
	{
		var nameMap = new Dictionary<string, ActiveUniformInfo>();
		var locationMap = new Dictionary<int, ActiveUniformInfo>();

		var count = GetActiveUniformCount();
		var maxLength = 0;
		GetProgrami(handle, ProgramPropertyARB.ActiveUniformMaxLength, ref maxLength);
		for (var location = 0; location < count; location++)
		{
			var length = 0;
			var uniformSize = 0;
			var uniformType = UniformType.Float;
			GetActiveUniform(handle, (uint)location, maxLength, ref length, ref uniformSize, ref uniformType,
				out var name);
			var info = new ActiveUniformInfo(location, name, uniformSize, uniformType);
			nameMap.Add(name, info);
			locationMap.Add(location, info);
		}

		return (nameMap.ToImmutableDictionary(), locationMap.ToImmutableDictionary());
	}

	public int GetActiveUniformCount()
	{
		var count = 0;
		GetProgrami(handle, ProgramPropertyARB.ActiveUniforms, ref count);
		return count;
	}

	public void SetUniform(int location, float value) => ProgramUniform1f(handle, location, value);
	public void SetUniform(int location, Vector3 value) => ProgramUniform3f(handle, location, value);
	public void SetUniform(int location, Vector4 value) => ProgramUniform4f(handle, location, value);

	public void SetUniform(int location, Matrix4 value) =>
		ProgramUniformMatrix4f(handle, location, transpose: false, in value);

	public bool TryGetUniform(string name, [MaybeNullWhen(returnValue: false)] out ActiveUniformInfo info) =>
		uniformNameMapper.TryGetValue(name, out info);

	public bool TryGetUniform(int location, [MaybeNullWhen(returnValue: false)] out ActiveUniformInfo info) =>
		uniformLocationMapper.TryGetValue(location, out info);

	public bool TryGetUniformLocation(string name, out int location)
	{
		if (!TryGetUniform(name, out var info))
		{
			location = 0;
			return false;
		}

		location = info.Location;
		return true;
	}

	public virtual void Bind()
	{
		UseProgram(handle);
	}

	public void Unbind()
	{
		UseProgram(ProgramHandle.Zero);
	}

	protected static ShaderHandle CompileShader(string sourceCode, ShaderType shaderType)
	{
		var shaderHandle = CreateShader(shaderType);
		ShaderSource(shaderHandle, InjectDefinesToShaderSource(sourceCode));
		GL.CompileShader(shaderHandle);

		var success = 0;
		GetShaderi(shaderHandle, ShaderParameterName.CompileStatus, ref success);
		// ReSharper disable once InvertIf
		if (success == GL_FALSE)
		{
			GetShaderInfoLog(shaderHandle, out var infoLog);
			throw new ShaderCompilationException(infoLog);
		}

		return shaderHandle;
	}

	protected override void DisposeNativeResources()
	{
		DeleteProgram(handle);
	}

	protected int GetAttributeLocation(string name) => GetAttribLocation(handle, name);

	/// <summary>
	///     Implicitly converts this shader program to its program handle. Useful for code snippets such as
	///     <c>GL.GetUniformLocation(shaderProgram, name: "material.highlightColor")</c>
	/// </summary>
	/// <param name="program">the program to convert to its program handle</param>
	/// <returns></returns>
	public static implicit operator ProgramHandle(ShaderProgram program) => program.handle;

	private static string InjectDefinesToShaderSource(string source)
	{
		var lines = Regex.Split(source, pattern: "\n").ToList();

		foreach (var location in Enum.GetValues<VertexAttributeLocation>().Reverse())
		{
			// The 0th item is the "#version ..." so we add the definitions after that
			lines.Insert(index: 1, $"#define {location.GetConstantName()} {(uint)location}");
		}

		return string.Join(separator: "\n", lines);
	}

	public record ActiveUniformInfo(int Location, string Name, int ByteSize, UniformType Type);

	public record UniformBlockData;
}