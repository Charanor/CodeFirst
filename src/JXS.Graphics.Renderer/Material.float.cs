using OpenTK.Graphics.OpenGL;

namespace JXS.Graphics.Renderer;

public partial record Material
{
	public float GetFloat(string propertyName) => GetFloat(GetPropertyId(propertyName));

	public float GetFloat(uint propertyId)
	{
		var registration = GetRegistration(propertyId);
		const UniformType expectedType = UniformType.Float;
		if (registration.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {registration.Type}, but expected {expectedType}");
		}

		return registration.AsFloat();
	}

	protected void SetFloat(string propertyName, float value) => SetFloat(GetPropertyId(propertyName), value);

	protected void SetFloat(uint propertyId, float value)
	{
		if (!ShaderProgram.TryGetUniform(propertyId, out var info))
		{
			throw new ArgumentException($"No uniform {propertyId} exists on shader", nameof(propertyId));
		}

		const UniformType expectedType = UniformType.Float;
		if (info.Type != expectedType)
		{
			throw new InvalidCastException(
				$"Property {propertyId} is of type {info.Type}, but expected {expectedType}");
		}

		registrations[propertyId] = new MaterialRegistration(info.Type, BitConverter.GetBytes(value));
	}
}