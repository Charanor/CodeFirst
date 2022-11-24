using JXS.Ecs.Core;
using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Attributes.Generation;

namespace JXS.Ecs.Examples._03_GeneratingCode;

[All(typeof(HelloWorldComponent))]
[ProcessPass(Pass.Update)]
public partial class HelloWorldSystem : IteratingSystem
{
	// By adding the [EntityProcessor] annotation, we can remove the Update(int, float) method as well as
	// the component mapper definition. This will all be added for us.

	[EntityProcessor]
	private void ProcessEntity(int entity, in HelloWorldComponent helloWorldC)
	{ 
		var worldString = "";
		for (var i = 0; i < helloWorldC.WorldCount; i++)
		{
			worldString += " World";
		}

		Console.WriteLine($@"Entity {entity} says: ""Hello{worldString}""");
	}
}