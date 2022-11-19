using OpenTK.Mathematics;

namespace JXS.Graphics.Renderer;

public partial record Material
{
	private readonly MaterialRegistration?[] registrations;

	public Material(ShaderProgram shaderProgram)
	{
		ShaderProgram = shaderProgram ?? throw new NullReferenceException();
		registrations = new MaterialRegistration[ShaderProgram.GetActiveUniformCount()];
	}

	public Material(Material parent)
	{
		ShaderProgram = parent.ShaderProgram;
		registrations = new MaterialRegistration[ShaderProgram.GetActiveUniformCount()];
	}

	public ShaderProgram ShaderProgram { get; }

	public void UploadUniformsToShader()
	{
		for (var location = 0u; location < registrations.Length; location++)
		{
			var registration = registrations[location];
			if (registration is null)
			{
				continue;
			}

			switch (registration.Type)
			{
				case UniformType.Float:
					ShaderProgram.SetUniform((int)location, registration.AsFloat());
					break;
				case UniformType.FloatVec3:
					ShaderProgram.SetUniform((int)location, registration.AsVector3Float());
					break;
				case UniformType.FloatVec4:
					ShaderProgram.SetUniform((int)location, registration.AsVector4Float());
					break;
				default:
					throw new NotImplementedException($"Uniforms of type {registration.Type} are not yet implemented.");
			}
		}
	}

	public void Bind()
	{
		ShaderProgram.Bind();
		UploadUniformsToShader();
	}

	public void Unbind()
	{
		ShaderProgram.Unbind();
	}

	public uint GetPropertyId(string propertyName) =>
		ShaderProgram.TryGetUniformLocation(propertyName, out var location)
			? location
			: throw new ArgumentException($"No uniform {propertyName} exists on shader", nameof(propertyName));

	private MaterialRegistration GetRegistration(uint propertyId)
	{
		if (propertyId >= registrations.Length)
		{
			throw new ArgumentException(
				$"{nameof(propertyId)} must be in range [0, {registrations.Length}], got {propertyId}",
				nameof(propertyId));
		}

		return registrations[propertyId] ??
		       throw new ArgumentException($"No parameter with id {propertyId} registered",
			       nameof(propertyId));
	}

	protected static string MaterialName(string inner) => $"material.{inner}";

	private record MaterialRegistration(UniformType Type, IEnumerable<byte> Data)
	{
		public float AsFloat()
		{
			if (Type != UniformType.Float || Data.Count() != sizeof(float))
			{
				throw new InvalidOperationException($"Can not convert {this} to float");
			}

			return BitConverter.ToSingle(Data.ToArray());
		}
		
		public int AsInt()
		{
			if (Type != UniformType.Int || Data.Count() != sizeof(int))
			{
				throw new InvalidOperationException($"Can not convert {this} to int");
			}

			return BitConverter.ToInt32(Data.ToArray());
		}

		public Vector3 AsVector3Float()
		{
			if (Type != UniformType.FloatVec3 || Data.Count() != Vector3.SizeInBytes)
			{
				throw new InvalidOperationException($"Can not convert {this} to {nameof(Vector3)}");
			}

			var data = Data.ToArray();
			return new Vector3(
				BitConverter.ToSingle(data, startIndex: 0),
				BitConverter.ToSingle(data, sizeof(float)),
				BitConverter.ToSingle(data, sizeof(float) * 2)
			);
		}

		public Vector4 AsVector4Float()
		{
			if (Type != UniformType.FloatVec4 || Data.Count() != Vector4.SizeInBytes)
			{
				throw new InvalidOperationException($"Can not convert {this} to {nameof(Vector4)}");
			}

			var data = Data.ToArray();
			return new Vector4(
				BitConverter.ToSingle(data, startIndex: 0),
				BitConverter.ToSingle(data, sizeof(float)),
				BitConverter.ToSingle(data, sizeof(float) * 2),
				BitConverter.ToSingle(data, sizeof(float) * 3)
			);
		}

		public Color4<Rgba> AsColor4()
		{
			if (Type != UniformType.FloatVec4 || Data.Count() != Vector4.SizeInBytes)
			{
				throw new InvalidOperationException($"Can not convert {this} to {nameof(Color4)}");
			}

			var data = Data.ToArray();
			return new Color4<Rgba>(
				BitConverter.ToSingle(data, startIndex: 0),
				BitConverter.ToSingle(data, sizeof(float)),
				BitConverter.ToSingle(data, sizeof(float) * 2),
				BitConverter.ToSingle(data, sizeof(float) * 3)
			);
		}
	}
}