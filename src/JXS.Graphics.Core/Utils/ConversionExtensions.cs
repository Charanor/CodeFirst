using OpenTK.Mathematics;

namespace JXS.Graphics.Core.Utils;

public static class ConversionExtensions
{
	public static Color4<TColorSpace> ToColor4<TColorSpace>(this Vector4 vec4) where TColorSpace : IColorSpace4 =>
		new(vec4.X, vec4.Y, vec4.Z, vec4.W);

	public static Vector4 ToVector4<TColorSpace>(this Color4<TColorSpace> color) where TColorSpace : IColorSpace4 =>
		new(color.X, color.Y, color.Z, color.W);

	public static Vector4 ToVector4(this Box2 box) => new(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
	public static Box2 ToBox2(this Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);

	public static byte[] ToByteArray<TColorSpace>(this Color4<TColorSpace> color) where TColorSpace : IColorSpace4
	{
		var (r, g, b, a) = color;
		return BitConverter.GetBytes(r)
			.Concat(BitConverter.GetBytes(g))
			.Concat(BitConverter.GetBytes(b))
			.Concat(BitConverter.GetBytes(a)).ToArray();
	}

	public static byte[] ToByteArray<TColorSpace>(this IEnumerable<Color4<TColorSpace>> colors)
		where TColorSpace : IColorSpace4 =>
		colors.SelectMany(color =>
		{
			var (r, g, b, a) = color;
			return BitConverter.GetBytes(r)
				.Concat(BitConverter.GetBytes(g))
				.Concat(BitConverter.GetBytes(b))
				.Concat(BitConverter.GetBytes(a));
		}).ToArray();
}