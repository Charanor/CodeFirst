namespace JXS.Ecs.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NoneAttribute : Attribute
{
	public NoneAttribute(params Type[] componentTypes)
	{
		Types = componentTypes;
	}

	public Type[] Types { get; }
}