using JXS.Ecs.Core;
using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Attributes.Generation;

namespace JXS.Ecs.Examples._03_GeneratingCode;

[All(typeof(HelloWorldComponent))]
[ProcessPass(Pass.Update)]
public partial class HelloWorldSystem : IteratingSystem
{
	// By adding the [EntityProcessor] annotation, we can remove the Update(int, float) method as well as
	// the component mapper definition.
	//
	// Note that the function can be called whatever as long as it is annotated with the [EntityProcessor] attribute.
	//
	// This function may optionally take any number of these parameters (in any order):
	// - a single "Entity": The currently processing entity. For the most part it is not necessary unless you need to
	//						check for optional components.
	// - a single "float": The current delta time.
	// - any number of "IComponent" derived types: The components you want to process.
	//
	// In addition, any "IComponent" derived types should also have either the "ref" or "in" modifier, especially
	// if your components are structs to make sure they are not copied by value and that you can actually modify them.
	// If it was not obvious:
	// - "in": Used when you *only* need to *read* the component.
	// - "ref": Used when you need to read and/or write to the component.
	//
	// It is a must to mark your component as "ref" if you modify it in any way, even if you are modifying one of its fields
	// (e.g. a list) since marking components with "in" allows the engine to do some optimizations that might silently
	// fail in multiple ways if you edit it.
	//
	// In the same vein, do not mark components you only read from as "ref" since that would prevent some useful
	// optimizations the engine can perform.

	[EntityProcessor]
	private void ProcessEntity(Entity entity, /* We only read, so mark as "in" */ in HelloWorldComponent helloWorldC)
	{
		var worldString = "";
		for (var i = 0; i < helloWorldC.WorldCount; i++)
		{
			worldString += " World";
		}

		Console.WriteLine($@"Entity {entity} says: ""Hello{worldString}""");
	}
}