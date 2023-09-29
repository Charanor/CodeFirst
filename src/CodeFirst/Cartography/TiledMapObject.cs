namespace CodeFirst.Cartography;

public class TiledMapObject
{
	/// <summary>
	///     The unique id of this object within this object's layer or group.
	/// </summary>
	public int Id { get; init; }

	/// <summary>
	///     The name of the object as set in the Tiled editor.
	/// </summary>
	/// <remarks>Defaults to <c>string.Empty</c>.</remarks>
	public string Name { get; init; }

	/// <summary>
	///     The class of the object.
	/// </summary>
	/// <remarks>Defaults to <c>string.Empty</c>.</remarks>
	public string Type { get; init; }

	/// <inheritdoc cref="Type" />
	/// <seealso cref="Type" />
	[Obsolete("Use Type instead")]
	public string Class => Type;

	/// <summary>
	///     The X position of this object, in pixels.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int X { get; init; }

	/// <summary>
	///     The Y position of this object, in pixels.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Y { get; init; }

	/// <summary>
	///     The width of this object, in pixels.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Width { get; init; }

	/// <summary>
	///     The height of this object, in pixels.
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public int Height { get; init; }

	/// <summary>
	///     The rotation of the object, in degrees, clockwise around (<see cref="X" />, <see cref="Y" />).
	/// </summary>
	/// <remarks>Defaults to <c>0</c>.</remarks>
	public float Rotation { get; init; }
	
	/// <summary>
	///		A reference to a tile.
	/// </summary>
	/// <remarks>Defaults to <c>0</c> if there is no reference to a tile.</remarks>
	public int Gid { get; init; }
	
	/// <summary>
	///		Whether the object is shown or hidden.
	/// </summary>
	/// <remarks>Defaults to <c>true</c>.</remarks>
	public bool Visible { get; init; }
	
	public required IReadOnlyDictionary<string, TiledMapProperty> Properties { get; init; }
	
	// TODO: Add template support

	public override string ToString() => $"{GetType().Name}-{Id} {Name}";
}