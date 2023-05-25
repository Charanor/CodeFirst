using JXS.Ecs.Core;
using JXS.Ecs.Core.Utilities;

namespace JXS.Ecs.Examples._04_AdvancedCodeGeneration;

internal class GeneratingCode
{
	private readonly World world;

	public GeneratingCode()
	{
		world = new World();
		CreateTestEntities();
		AddSystems();
		RunOnce();
	}

	private void CreateTestEntities()
	{
		var random = new Random();
		var entityCount = random.Next(minValue: 3, maxValue: 6);
		Console.WriteLine($"Creating {entityCount} entities");
		for (var i = 0; i < entityCount; i++)
		{
			var entity = new EntityBuilder(world);
			entity.Add(new HelloWorldComponent
			{
				WorldCount = random.Next(minValue: 0, maxValue: 5)
			});
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
		new GeneratingCode();
	}
}