using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public partial record Material
{
	public Vector4 GetVector4F(string propertyName) => GetVector4F(GetPropertyId(propertyName));

	public Vector4 GetVector4F(int propertyId)
	{
		var registration = GetRegistration(propertyId);
		const UniformType expectedType = UniformType.FloatVec4;
		if (registration.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {registration.Type}, but expected {expectedType}");
		}

		return registration.AsVector4Float();
	}

	protected void SetVector4F(string propertyName, Vector4 value) => SetVector4F(GetPropertyId(propertyName), value);

	protected void SetVector4F(int propertyId, Vector4 value)
	{
		if (!ShaderProgram.TryGetUniform(propertyId, out var info))
		{
			throw new ArgumentException($"No uniform {propertyId} exists on shader", nameof(propertyId));
		}

		const UniformType expectedType = UniformType.FloatVec4;
		if (info.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {info.Type}, but expected {expectedType}");
		}

		var data = BitConverter.GetBytes(value.X)
			.Concat(BitConverter.GetBytes(value.Y))
			.Concat(BitConverter.GetBytes(value.Z))
			.Concat(BitConverter.GetBytes(value.W))
			.ToArray();

		registrations[propertyId] = new MaterialRegistration(info.Type, data);
	}
}