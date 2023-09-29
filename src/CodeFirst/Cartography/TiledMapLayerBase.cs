using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Cartography;

public class TiledMapLayerBase
{
	/// <summary>
	///     The unique id of this layer.
	/// </summary>
	public required int Id { get; init; }

	/// <summary>
	///     The name of this layer as set in the Tiled editor.
	/// </summary>
	/// <remarks>Defaults to <c>string.Empty</c>.</remarks>
	public required string Name { get; init; }

	/// <summary>
	///     The opacity of the layer.
	/// </summary>
	/// <remarks>In the range <c>[0, 1]</c>.</remarks>
	[ValueRange(from: 0, to: 1)]
	public float Opacity { get; init; } = 1;

	/// <summary>
	///     If the layer is shown or hidden.
	/// </summary>
	/// <remarks>Defaults to <c>true</c>.</remarks>
	public bool Visible { get; init; } = true;

	/// <summary>
	///     A tint color that is multiplied with any tiles drawn by this layer.
	/// </summary>
	/// <remarks>Defaults to <c>null</c> (i.e. "no tint").</remarks>
	public Color4<Rgba>? TintColor { get; init; } = null;

	/// <summary>
	///     Freeform properties of this layer as set in the Tiled editor.
	/// </summary>
	public required IReadOnlyDictionary<string, TiledMapProperty> Properties { get; init; }

	public override string ToString() => $"{GetType().Name}-{Id} {Name}";
}