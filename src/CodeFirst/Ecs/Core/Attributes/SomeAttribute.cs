using JetBrains.Annotations;

namespace CodeFirst.Ecs.Core.Attributes;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public sealed class SomeAttribute : Attribute
{
	public SomeAttribute(params Type[] componentTypes)
	{
		Types = componentTypes;
	}

	public Type[] Types { get; }
}