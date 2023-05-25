namespace JXS.Ecs.Core;

public static class ComponentManager
{
	private const int DEFAULT_COMPONENT_COUNT = 32;

	private static int nextId;
	private static Type[] componentTypes = new Type[DEFAULT_COMPONENT_COUNT];

	// SIC! The number of registered types is just the next id :)
	internal static int NumTypes => nextId;

	internal static int GetId(Type type)
	{
		lock (componentTypes)
		{
			// No LINQ! This method can potentially be called a lot, let's not create new objects for no reason
			for (var i = 0; i < componentTypes.Length; i++)
			{
				if (componentTypes[i] == type)
				{
					return i;
				}
			}

			// Component not registered yet, register it
			var id = nextId;
			Interlocked.Increment(ref nextId); // Probably no needed since inside lock() block but you know... safety :P
			if (id >= componentTypes.Length)
			{
				Array.Resize(ref componentTypes, componentTypes.Length * 2);
			}

			componentTypes[id] = type;
			return id;
		}
	}

	internal static int GetId<T>() where T : IComponent => GetId(typeof(T));

	public static Type GetType(int id)
	{
		lock (componentTypes)
		{
			return componentTypes[id];
		}
	}
}