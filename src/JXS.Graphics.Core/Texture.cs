using System.Collections.Immutable;
using System.ComponentModel;
using OpenTK.Mathematics;

namespace JXS.Graphics.Core;

public abstract class Texture : NativeResource
{
	public static readonly TextureSlot Unit0 = new(TextureUnit.Texture0);

	private readonly TextureMinFilter minFilter;
	private readonly TextureMagFilter magFilter;
	private readonly bool mipmap;
	private readonly TextureWrapMode wrapS;
	private readonly TextureWrapMode wrapT;
	private readonly TextureWrapMode wrapR;

	protected unsafe Texture(TextureTarget target, ReadOnlySpan<byte> data, int width, int height,
		int depth, int mipMapLevels = 1, SizedInternalFormat internalFormat = SizedInternalFormat.Rgba8,
		PixelFormat format = PixelFormat.Rgba, PixelType type = PixelType.Float)
	{
		if (!Enum.IsDefined(target))
		{
			throw new InvalidEnumArgumentException(nameof(target), (int)target, typeof(TextureTarget));
		}

		Target = target;
		Handle = CreateTexture(Target);
		Data = data.ToArray().ToImmutableArray();

		Width = width;
		Height = height;
		Depth = depth;
		Dimensions = new Vector3i(width, height, depth);

		fixed (byte* ptr = data)
		{
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (target)
			{
				case TextureTarget.Texture1d:
					EnsureValidDimensions(width);
					TextureStorage1D(this, mipMapLevels, internalFormat, width);
					TextureSubImage1D(this, level: 0, xoffset: 0, width, format, type, ptr);
					break;
				case TextureTarget.Texture2d:
					EnsureValidDimensions(width, height);
					TextureStorage2D(this, mipMapLevels, internalFormat, width, height);
					TextureSubImage2D(this, level: 0, xoffset: 0, yoffset: 0, width, height, format, type, ptr);
					break;
				case TextureTarget.Texture3d:
					EnsureValidDimensions(width, height, depth);
					TextureStorage3D(this, mipMapLevels, internalFormat, width, height, depth);
					TextureSubImage3D(this, level: 0, xoffset: 0, yoffset: 0, zoffset: 0, width, height, depth, format,
						type, ptr);
					break;
				default:
					if (!Enum.IsDefined(target))
					{
						// Invalid enum value
						throw new InvalidEnumArgumentException(nameof(target), (int)target, typeof(TextureTarget));
					}

					// We simply can not process this type of texture
					throw new InvalidEnumArgumentException($"Unable to create texture for {nameof(TextureTarget)}.{Enum.GetName(target)} = ({target}): Unsupported target.");
			}
		}
	}

	public TextureHandle Handle { get; }
	public TextureTarget Target { get; }
	public ImmutableArray<byte> Data { get; }

	public int Width { get; }
	public int Height { get; }
	public int Depth { get; }
	public Vector3i Dimensions { get; }

	public TextureMinFilter MinFilter
	{
		get => minFilter;
		init
		{
			if (!Enum.IsDefined(value))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextureMinFilter));
			}

			minFilter = value;
			TextureParameteri(this, TextureParameterName.TextureMinFilter, (int)value);
		}
	}

	public TextureMagFilter MagFilter
	{
		get => magFilter;
		init
		{
			if (!Enum.IsDefined(value))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextureMagFilter));
			}

			magFilter = value;
			TextureParameteri(this, TextureParameterName.TextureMagFilter, (int)value);
		}
	}

	// 1d+
	public TextureWrapMode WrapS
	{
		get => wrapS;
		init
		{
			if (!Enum.IsDefined(value))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextureWrapMode));
			}

			wrapS = value;
			TextureParameteri(this, TextureParameterName.TextureWrapS, (int)value);
		}
	}

	// 2d+
	public TextureWrapMode WrapT
	{
		get => wrapT;
		init
		{
			if (Target == TextureTarget.Texture1d)
			{
				// We can't support wrap T, throw!
				throw new InvalidOperationException("Can not set texture wrap for 2:nd dimension T for a 1d texture");
			}

			if (!Enum.IsDefined(value))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextureWrapMode));
			}

			wrapT = value;
			TextureParameteri(this, TextureParameterName.TextureWrapT, (int)value);
		}
	}

	// 3d
	public TextureWrapMode WrapR
	{
		get => wrapR;
		init
		{
			if (Target != TextureTarget.Texture3d)
			{
				// We can't support wrap R, throw!
				throw new InvalidOperationException(
					"Can not set texture wrap for 3rd dimension T for a 1d or 2d texture");
			}

			if (!Enum.IsDefined(value))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(TextureWrapMode));
			}

			wrapR = value;
			TextureParameteri(this, TextureParameterName.TextureWrapR, (int)value);
		}
	}

	public bool Mipmap
	{
		get => mipmap;
		init
		{
			mipmap = value;
			if (value)
			{
				GenerateTextureMipmap(this);
			}
		}
	}

	protected override void DisposeNativeResources()
	{
		DeleteTexture(this);
	}

	public static implicit operator TextureHandle(Texture texture) => texture.Handle;

	protected bool Equals(Texture other) => Handle.Equals(other.Handle);

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(objA: null, obj))
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		return obj.GetType() == GetType() && Equals((Texture)obj);
	}

	public override int GetHashCode() => Handle.GetHashCode();

	public static bool operator ==(Texture? left, Texture? right) => Equals(left, right);

	public static bool operator !=(Texture? left, Texture? right) => !Equals(left, right);

	private static void EnsureValidDimensions(int width, int? height = null, int? depth = null)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(width));
		}

		if (height is <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(height));
		}

		if (depth is <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(depth));
		}
	}

	public sealed class TextureBinding : IDisposable
	{
		private readonly Texture? texture;
		private bool isDisposed;

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
			
			GC.SuppressFinalize(this);
			isDisposed = true;
			if (texture == null || !Enum.IsDefined(Unit))
			{
				return;
			}

			ActiveTexture(Unit);
			BindTexture(texture.Target, texture);
		}

		~TextureBinding()
		{
			Dispose();
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