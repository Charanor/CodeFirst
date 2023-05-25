using JXS.Ecs.Core;

namespace JXS.Ecs.Examples._04_AdvancedCodeGeneration;

internal struct HelloWorldComponent : IComponent
{
	public int WorldCount { get; set; }
}