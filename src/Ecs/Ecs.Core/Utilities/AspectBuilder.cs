namespace JXS.Ecs.Core.Utilities;

/// <summary>
///     A utility class for creating <see cref="Aspect" />s.
/// </summary>
/// <seealso cref="ComponentFlagsBuilder"/>
/// <seealso cref="Aspect"/>
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

	public AspectBuilder All(params Type[] types)
	{
		foreach (var type in types)
		{
			all.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder All<T>() => All(typeof(T));

	public AspectBuilder All<T1, T2>() => All(typeof(T1), typeof(T2));

	public AspectBuilder All<T1, T2, T3>() => All(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder All<T1, T2, T3, T4>() => All(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder All<T1, T2, T3, T4, T5>() => All(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public AspectBuilder Some(params Type[] types)
	{
		foreach (var type in types)
		{
			some.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder Some<T>() => Some(typeof(T));

	public AspectBuilder Some<T1, T2>() => Some(typeof(T1), typeof(T2));

	public AspectBuilder Some<T1, T2, T3>() => Some(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder Some<T1, T2, T3, T4>() => Some(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder Some<T1, T2, T3, T4, T5>() => Some(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public AspectBuilder None(params Type[] types)
	{
		foreach (var type in types)
		{
			none.Enable(ComponentManager.GetId(type));
		}

		return this;
	}

	public AspectBuilder None<T>() => None(typeof(T));

	public AspectBuilder None<T1, T2>() => None(typeof(T1), typeof(T2));

	public AspectBuilder None<T1, T2, T3>() => None(typeof(T1), typeof(T2), typeof(T3));

	public AspectBuilder None<T1, T2, T3, T4>() => None(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

	public AspectBuilder None<T1, T2, T3, T4, T5>() => None(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

	public Aspect Build() => new(all, some, none);

	public static implicit operator Aspect(AspectBuilder builder) => builder.Build();
}