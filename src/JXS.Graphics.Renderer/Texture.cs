namespace JXS.Graphics.Renderer;

public class Texture : NativeResource
{
	public static readonly TextureSlot Unit0 = new(TextureUnit.Texture0);

	public Texture(TextureTarget target)
	{
		Target = target;
		Handle = CreateTexture(Target);
	}

	public TextureHandle Handle { get; }
	public TextureTarget Target { get; }

	protected override void DisposeNativeResources()
	{
		DeleteTexture(this);
	}

	public static implicit operator TextureHandle(Texture texture) => texture.Handle;

	public class TextureBinding : IDisposable
	{
		private bool isDisposed;
		private readonly Texture? texture;

		public TextureBinding()
		{
			texture = null;
			Unit = 0; // 0 is an invalid unit
		}

		~TextureBinding()
		{
			Dispose();
		}

		public TextureBinding(Texture texture, TextureUnit unit)
		{
			this.texture = texture;
			Unit = unit;
			ActiveTexture(unit);
			BindTexture(texture.Target, texture);
		}

		public TextureUnit Unit { get; }

		public void Dispose()
		{
			if (isDisposed)
			{
				return;
			}

			isDisposed = true;
			if (texture == null || !Enum.IsDefined(Unit))
			{
				return;
			}
			
			ActiveTexture(Unit);
			BindTexture(texture.Target, texture);
		}
	}

	public class TextureSlot
	{
		internal TextureSlot(TextureUnit unit)
		{
			Unit = unit;
		}

		public TextureUnit Unit { get; }

		public TextureBinding Bind(Texture texture) => new(texture, Unit);
	}
}