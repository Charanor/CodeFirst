using JetBrains.Annotations;

namespace CodeFirst.Ecs.Core.Attributes;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public sealed class NoneAttribute : Attribute
{
	public NoneAttribute(params Type[] componentTypes)
	{
		Types = componentTypes;
	}

	public Type[] Types { get; }
}