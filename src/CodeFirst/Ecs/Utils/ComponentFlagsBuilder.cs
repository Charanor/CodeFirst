﻿using CodeFirst.Ecs.Core;

namespace CodeFirst.Ecs.Utils;

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

	public ComponentFlagsBuilder(ComponentFlags baseFlags)
	{
		flags = new bool[ComponentManager.NumTypes];
		for (var i = 0; i < flags.Length; i++)
		{
			flags[i] = baseFlags.Has(i);
		}
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