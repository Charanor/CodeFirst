using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using JXS.Graphics.Renderer.Exceptions;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace JXS.Graphics.Renderer;

public class ShaderProgram : NativeResource
{
	// ReSharper disable once InconsistentNaming
	protected const int GL_FALSE = 0;

	private readonly ProgramHandle handle;

	private readonly IImmutableDictionary<string, ActiveUniformInfo> uniformNameMapper;
	private readonly IImmutableDictionary<uint, ActiveUniformInfo> uniformLocationMapper;

	public ShaderProgram(string vertexShaderSource, string fragmentShaderSource)
	{
		handle = GL.CreateProgram();

		var vertexShaderId = CompileShader(vertexShaderSource, ShaderType.VertexShader);
		GL.AttachShader(handle, vertexShaderId);

		var fragmentShaderId = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);
		GL.AttachShader(handle, fragmentShaderId);

		GL.LinkProgram(handle);
		var success = 0;
		GL.GetProgrami(handle, ProgramPropertyARB.LinkStatus, ref success);
		if (success == GL_FALSE)
		{
			GL.GetProgramInfoLog(handle, out var infoLog);
			throw new ShaderCompilationException(infoLog);
		}

		GL.DetachShader(handle, vertexShaderId);
		GL.DeleteShader(vertexShaderId);

		GL.DetachShader(handle, fragmentShaderId);
		GL.DeleteShader(fragmentShaderId);

		(uniformNameMapper, uniformLocationMapper) = CreateUniformMappers();
	}

	private (IImmutableDictionary<string, ActiveUniformInfo>, IImmutableDictionary<uint, ActiveUniformInfo>)
		CreateUniformMappers()
	{
		var nameMap = new Dictionary<string, ActiveUniformInfo>();
		var locationMap = new Dictionary<uint, ActiveUniformInfo>();

		var count = GetActiveUniformCount();
		var maxLength = 0;
		GL.GetProgrami(handle, ProgramPropertyARB.ActiveUniformMaxLength, ref maxLength);
		for (uint location = 0; location < count; location++)
		{
			var length = 0;
			var uniformSize = 0;
			var uniformType = UniformType.Float;
			GL.GetActiveUniform(handle, location, maxLength, ref length, ref uniformSize, ref uniformType,
				out var name);
			var info = new ActiveUniformInfo(location, name, uniformSize, uniformType);
			nameMap.Add(name, info);
			locationMap.Add(location, info);
		}

		return (nameMap.ToImmutableDictionary(), locationMap.ToImmutableDictionary());
	}

	private IImmutableDictionary<string, ActiveUniformInfo> CreateUniformNameMapper()
	{
		var map = new Dictionary<string, ActiveUniformInfo>();

		var count = GetActiveUniformCount();
		var maxLength = 0;
		GL.GetProgrami(handle, ProgramPropertyARB.ActiveUniformMaxLength, ref maxLength);
		for (uint i = 0; i < count; i++)
		{
			var length = 0;
			var uniformSize = 0;
			var uniformType = UniformType.Float;
			GL.GetActiveUniform(handle, i, maxLength, ref length, ref uniformSize, ref uniformType, out var name);
			map.Add(name, new ActiveUniformInfo(i, name, uniformSize, uniformType));
		}

		return map.ToImmutableDictionary();
	}

	public int GetActiveUniformCount()
	{
		var count = 0;
		GL.GetProgrami(handle, ProgramPropertyARB.ActiveUniforms, ref count);
		return count;
	}

	public void SetUniform(int location, float value) => GL.ProgramUniform1f(handle, location, value);
	public void SetUniform(int location, Vector3 value) => GL.ProgramUniform3f(handle, location, value);
	public void SetUniform(int location, Vector4 value) => GL.ProgramUniform4f(handle, location, value);

	public void SetUniform(int location, Matrix4 value) =>
		GL.ProgramUniformMatrix4f(handle, location, transpose: false, in value);

	public bool TryGetUniform(string name, [MaybeNullWhen(returnValue: false)] out ActiveUniformInfo info) =>
		uniformNameMapper.TryGetValue(name, out info);

	public bool TryGetUniform(uint location, [MaybeNullWhen(returnValue: false)] out ActiveUniformInfo info) =>
		uniformLocationMapper.TryGetValue(location, out info);

	public bool TryGetUniformLocation(string name, out uint location)
	{
		if (!TryGetUniform(name, out var info))
		{
			location = 0;
			return false;
		}

		location = info.Location;
		return true;
	}

	public void Bind()
	{
		GL.UseProgram(handle);
	}

	public void Unbind()
	{
		GL.UseProgram(ProgramHandle.Zero);
	}

	protected static ShaderHandle CompileShader(string sourceCode, ShaderType shaderType)
	{
		var shaderHandle = GL.CreateShader(shaderType);
		GL.ShaderSource(shaderHandle, InjectDefinesToShaderSource(sourceCode));
		Console.WriteLine(InjectDefinesToShaderSource(sourceCode));
		GL.CompileShader(shaderHandle);

		var success = 0;
		GL.GetShaderi(shaderHandle, ShaderParameterName.CompileStatus, ref success);
		// ReSharper disable once InvertIf
		if (success == GL_FALSE)
		{
			GL.GetShaderInfoLog(shaderHandle, out var infoLog);
			throw new ShaderCompilationException(infoLog);
		}

		return shaderHandle;
	}

	protected override void DisposeNativeResources()
	{
		GL.DeleteProgram(handle);
	}

	protected int GetAttributeLocation(string name) => GL.GetAttribLocation(handle, name);

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

	public record ActiveUniformInfo(uint Location, string Name, int ByteSize, UniformType Type);
}