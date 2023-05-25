using JXS.Ecs.Core;

namespace JXS.Ecs.Examples._04_AdvancedCodeGeneration;

internal struct IterationCounter : IComponent
{
	public int Count { get; set; }
}