using OpenTK.Graphics.OpenGL;

namespace JXS.Graphics.Renderer;

public partial record Material
{
	public float GetInt(string propertyName) => GetInt(GetPropertyId(propertyName));

	public float GetInt(uint propertyId)
	{
		var registration = GetRegistration(propertyId);
		const UniformType expectedType = UniformType.Int;
		if (registration.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {registration.Type}, but expected {expectedType}");
		}

		return registration.AsInt();
	}

	protected void SetInt(string propertyName, int value) => SetInt(GetPropertyId(propertyName), value);

	protected void SetInt(uint propertyId, int value)
	{
		if (!ShaderProgram.TryGetUniform(propertyId, out var info))
		{
			throw new ArgumentException($"No uniform {propertyId} exists on shader", nameof(propertyId));
		}

		const UniformType expectedType = UniformType.Int;
		if (info.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {info.Type}, but expected {expectedType}");
		}

		registrations[propertyId] = new MaterialRegistration(info.Type, BitConverter.GetBytes(value));
	}
}