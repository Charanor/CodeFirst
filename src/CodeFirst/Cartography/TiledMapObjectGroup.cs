namespace CodeFirst.Cartography;

public class TiledMapObjectGroup : TiledMapLayerBase
{
	/// <summary>
	///     The draw order of objects inside this group.
	/// </summary>
	public TiledMapDrawOrder DrawOrder { get; init; } = TiledMapDrawOrder.Index;
	
	public required IReadOnlyCollection<TiledMapObject> Objects { get; init;  }
}