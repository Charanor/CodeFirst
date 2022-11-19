using OpenTK.Mathematics;

namespace JXS.Graphics.Renderer;

public partial record Material
{
	public Vector3 GetVector3F(string propertyName) => GetVector3F(GetPropertyId(propertyName));

	public Vector3 GetVector3F(uint propertyId)
	{
		var registration = GetRegistration(propertyId);
		const UniformType expectedType = UniformType.FloatVec3;
		if (registration.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {registration.Type}, but expected {expectedType}");
		}

		return registration.AsVector3Float();
	}

	protected void SetVector3F(string propertyName, Vector3 value) => SetVector3F(GetPropertyId(propertyName), value);

	protected void SetVector3F(uint propertyId, Vector3 value)
	{
		if (!ShaderProgram.TryGetUniform(propertyId, out var info))
		{
			throw new ArgumentException($"No uniform {propertyId} exists on shader", nameof(propertyId));
		}

		const UniformType expectedType = UniformType.FloatVec3;
		if (info.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {info.Type}, but expected {expectedType}");
		}

		var data = BitConverter.GetBytes(value.X)
			.Concat(BitConverter.GetBytes(value.Y))
			.Concat(BitConverter.GetBytes(value.Z))
			.ToArray();

		registrations[propertyId] = new MaterialRegistration(info.Type, data);
	}
}