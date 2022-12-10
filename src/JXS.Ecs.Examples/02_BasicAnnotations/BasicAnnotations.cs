using JXS.Ecs.Core;
using JXS.Ecs.Core.Attributes;
using JXS.Ecs.Core.Utilities;

namespace JXS.Ecs.Examples._02_BasicAnnotations;

internal class BasicAnnotations
{
	private readonly World world;

	public BasicAnnotations()
	{
		world = new World();
		CreateTestEntities();
		AddSystems();
		RunOnce();
	}

	private void CreateTestEntities()
	{
		var random = new Random();
		var entityCount = random.Next(3, 6);
		Console.WriteLine($"Creating {entityCount} entities");
		for (var i = 0; i < entityCount; i++)
		{
			var entity = new EntityBuilder(world);
			entity.Add(new HelloWorldComponent(random.Next(0, 5)));
		}
	}

	private void AddSystems()
	{
		world.AddSystem(new HelloWorldSystem());
	}

	private void RunOnce()
	{
		world.Update(1f / 120f);
		world.Draw(1f / 120f);
	}

	public static void Main()
	{
		new BasicAnnotations();
	}

	private record HelloWorldComponent(int WorldCount) : IComponent;

	// We replaced the constructor with these two annotations
	[All(typeof(HelloWorldComponent))]
	[ProcessPass(Pass.Update)]
	private class HelloWorldSystem : IteratingSystem
	{
		private readonly ComponentMapper<HelloWorldComponent> helloWorldMapper = null!;

		protected override void Update(Entity entity, float delta)
		{
			var helloWorldC = helloWorldMapper.Get(entity);

			var worldString = "";
			for (var i = 0; i < helloWorldC.WorldCount; i++)
			{
				worldString += " World";
			}

			Console.WriteLine($@"Entity {entity} says: ""Hello{worldString}""");
		}
	}
}