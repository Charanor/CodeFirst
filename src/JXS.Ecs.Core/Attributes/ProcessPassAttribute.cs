namespace JXS.Ecs.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ProcessPassAttribute : Attribute
{
	public ProcessPassAttribute(Pass pass)
	{
		Pass = pass;
	}

	public Pass Pass { get; }
}