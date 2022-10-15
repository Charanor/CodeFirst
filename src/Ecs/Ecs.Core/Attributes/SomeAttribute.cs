namespace JXS.Ecs.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SomeAttribute : Attribute
{
	public SomeAttribute(params Type[] componentTypes)
	{
		Types = componentTypes;
	}

	public Type[] Types { get; }
}