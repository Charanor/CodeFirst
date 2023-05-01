using System;

namespace Ecs.Generators.Utils;

public static class ClassBuilderExtensions
{
	public static IDisposable Block(this ClassBuilder builder, string text) => new DisposableBlock(text, builder);

	private record DisposableBlock : IDisposable
	{
		private readonly ClassBuilder builder;
		
		public DisposableBlock(string text, ClassBuilder builder)
		{
			this.builder = builder;
			builder.BeginBlock(text);
		}
		
		public void Dispose()
		{
			builder.EndBlock();
		}
	}
}
