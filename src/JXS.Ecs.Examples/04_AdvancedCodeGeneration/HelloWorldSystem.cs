using JXS.Ecs.Core;
using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Attributes.Generation;

namespace JXS.Ecs.Examples._04_AdvancedCodeGeneration;

[All(typeof(HelloWorldComponent))]
[ProcessPass(Pass.Update)]
// Here we add another attribute, "GenerateComponentUtilities". This attribute takes a list of component types and
// generates some utility methods and fields, such as:
//  - (Property) bool Has{ComponentType}: Indicates if the current entity has the given component or not.
//  - (Property) ref {ComponentType} {ComponentType}: Contains a reference to the component. If the entity does not have a
//												      component of that type, the reference is invalid and should NOT be
//												      used or bad things can happen!
//  - (Methods) ref {ComponentType} Create{ComponentType}: Creates a component of that type for this entity and returns it.
//
// For the complete list see the full documentation.
[GenerateComponentUtilities(typeof(IterationCounter))]
public partial class HelloWorldSystem : IteratingSystem
{
	[EntityProcessor]
	private void ProcessEntity(Entity entity, in HelloWorldComponent helloWorldC)
	{
		var worldString = "";
		for (var i = 0; i < helloWorldC.WorldCount; i++)
		{
			worldString += " World";
		}

		// (Property) bool Has{ComponentType}
		if (HasIterationCounter)
		{
			// (Property) ref {ComponentType} {ComponentType}
			IterationCounter.Count += 1;
			Console.WriteLine(
				$@"Entity {entity} says: ""Hello{worldString}"". It has said this {IterationCounter.Count} times.");
		}
		else
		{
			Console.WriteLine($@"Entity {entity} says: ""Hello{worldString}"".");
		}
	}
}