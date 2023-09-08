using System.Reflection;
using CodeFirst.Ecs.Core.Attributes;

namespace CodeFirst.Ecs.Core.Utilities;

/// <summary>
///     A utility class for creating <see cref="Aspect" />s.
/// </summary>
/// <seealso cref="ComponentFlagsBuilder" />
/// <seealso cref="Aspect" />
public class AspectBuilder
{
	private readonly ComponentFlagsBuilder all;
	private readonly ComponentFlagsBuilder some;
	private readonly ComponentFlagsBuilder none;

	public AspectBuilder()
	{
		all = new ComponentFlagsBuilder();
		some = new ComponentFlagsBuilder();
		none = new ComponentFlagsBuilder();
	}

	public AspectBuilder(Aspect baseAspect)
	{
		all = new ComponentFlagsBuilder(baseAspect.All);
		some = new ComponentFlagsBuilder(baseAspect.Some);
		none = new ComponentFlagsBuilder(baseAspect.None);
	}

	public AspectBuilder All(params Type[] types)
	{
		foreach (var type in types)
		{
			all.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder All<T>() where T : IComponent => All(typeof(T));

	public AspectBuilder All<T1, T2>() where T1 : IComponent where T2 : IComponent => All(typeof(T1), typeof(T2));

	public AspectBuilder All<T1, T2, T3>() where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		All(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder All<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		All(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder All<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		All(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public AspectBuilder Some(params Type[] types)
	{
		foreach (var type in types)
		{
			some.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder Some<T>() where T : IComponent => Some(typeof(T));

	public AspectBuilder Some<T1, T2>() where T1 : IComponent where T2 : IComponent => Some(typeof(T1), typeof(T2));

	public AspectBuilder Some<T1, T2, T3>() where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		Some(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder Some<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		Some(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder Some<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		Some(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public AspectBuilder None(params Type[] types)
	{
		foreach (var type in types)
		{
			none.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder None<T>() where T : IComponent => None(typeof(T));

	public AspectBuilder None<T1, T2>() where T1 : IComponent where T2 : IComponent => None(typeof(T1), typeof(T2));

	public AspectBuilder None<T1, T2, T3>() where T1 : IComponent where T2 : IComponent where T3 : IComponent =>
		None(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder None<T1, T2, T3, T4>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent =>
		None(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder None<T1, T2, T3, T4, T5>()
		where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent =>
		None(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public Aspect Build() => new(all, some, none);

	public static implicit operator Aspect(AspectBuilder builder) => builder.Build();

	public static Aspect GetAspectFromAttributes(Type type)
	{
		var all = type.GetCustomAttribute<AllAttribute>();
		var one = type.GetCustomAttribute<SomeAttribute>();
		var none = type.GetCustomAttribute<NoneAttribute>();

		var builder = new AspectBuilder();
		builder.All(all?.Types ?? Array.Empty<Type>());
		builder.Some(one?.Types ?? Array.Empty<Type>());
		builder.None(none?.Types ?? Array.Empty<Type>());
		return builder.Build();
	}

	public static Aspect GetAspectFromFieldAttributes(FieldInfo fieldInfo)
	{
		var all = fieldInfo.GetCustomAttribute<AllAttribute>();
		var one = fieldInfo.GetCustomAttribute<SomeAttribute>();
		var none = fieldInfo.GetCustomAttribute<NoneAttribute>();

		var builder = new AspectBuilder();
		builder.All(all?.Types ?? Array.Empty<Type>());
		builder.Some(one?.Types ?? Array.Empty<Type>());
		builder.None(none?.Types ?? Array.Empty<Type>());
		return builder.Build();
	}
}