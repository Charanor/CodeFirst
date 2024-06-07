using JetBrains.Annotations;

namespace CodeFirst.Ecs.Core.Attributes;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class ProcessPassAttribute : Attribute
{
	public ProcessPassAttribute(Pass pass)
	{
		Pass = pass;
	}

	public Pass Pass { get; }
}