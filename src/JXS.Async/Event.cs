namespace JXS.Async;

public class Event : IEquatable<Event>
{
	private readonly Guid uuid;

	public Event()
	{
		uuid = Guid.NewGuid();
	}

	public bool Equals(Event? other)
	{
		if (ReferenceEquals(objA: null, other))
		{
			return false;
		}

		return ReferenceEquals(this, other) || uuid.Equals(other.uuid);
	}

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

		return obj.GetType() == GetType() && Equals((Event)obj);
	}

	public override int GetHashCode() => uuid.GetHashCode();

	public static bool operator ==(Event? left, Event? right) => Equals(left, right);

	public static bool operator !=(Event? left, Event? right) => !Equals(left, right);
}