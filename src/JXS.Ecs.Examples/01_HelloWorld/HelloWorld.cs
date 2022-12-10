using JXS.Ecs.Core;
using JXS.Ecs.Core.Utilities;

namespace JXS.Ecs.Examples._01_HelloWorld;

internal class HelloWorld
{
	private readonly World world;

	public HelloWorld()
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
			// The EntityBuilder is a utility class that simplifies creation of entities. Note that the EntityBuilder
			// is VERY slow compared to manually building an entity, but for prototyping it's quite useful.
			// The EntityBuilder automatically creates a new entity and adds it to the world when constructed.
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
		// The value given here is the time since the last call to "Update" and "Draw". This is usually called a
		// "delta time" in game engines and will be given to your EntitySystems in the "delta" parameter.
		world.Update(1f / 120f);

		// We don't need this since we don't have any systems that update during the "Draw" step, but shown here for
		// completeness sake
		world.Draw(1f / 120f);
	}

	public static void Main()
	{
		var entity = default(Entity);
		Console.WriteLine(entity.IsValid);
		Console.WriteLine(entity);
		
		new HelloWorld();
	}

	private record HelloWorldComponent(int WorldCount) : IComponent;

	private class HelloWorldSystem : IteratingSystem
	{
		// A ComponentMapper is used to map an entity to a specific component. It can be used to add, remove, or modify
		// an instance of its component (in this case; HelloWorldComponent) given an entity.
		//
		// All ComponentMapper instances are injected when the system is first added to the World. Make sure that the
		// member is set to "readonly"! Initializing to "null!" is not necessary but suppresses warnings.
		private readonly ComponentMapper<HelloWorldComponent> helloWorldMapper = null!;

		public HelloWorldSystem() : base(
			// Only process entities that have the HelloWorldComponent
			new AspectBuilder().All<HelloWorldComponent>(),
			// Process entities during the "Update" step
			Pass.Update)
		{
		}

		protected override void Update(Entity entity, float delta)
		{
			// Naming convention is to add "C" as a suffix to component instance variables.
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