namespace JXS.Ecs.Core.Utilities;

/// <summary>
///     A utility function for creating <see cref="ComponentFlags" />.
/// </summary>
/// <seealso cref="AspectBuilder" />
/// <seealso cref="ComponentFlags" />
public class ComponentFlagsBuilder
{
	private bool[] flags;

	public ComponentFlagsBuilder()
	{
		flags = new bool[ComponentManager.NumTypes];
	}

	public void Set(int componentId, bool state)
	{
		EnsureHasSpaceForComponentId(componentId);
		flags[componentId] = state;
	}

	public void Enable(int componentId) => Set(componentId, state: true);

	public void Disable(int componentId) => Set(componentId, state: false);

	private void EnsureHasSpaceForComponentId(int componentId)
	{
		if (componentId >= flags.Length)
		{
			Array.Resize(ref flags, componentId + 1);
		}
	}

	public ComponentFlags Build() => new(flags);

	public static implicit operator ComponentFlags(ComponentFlagsBuilder builder) => builder.Build();
}