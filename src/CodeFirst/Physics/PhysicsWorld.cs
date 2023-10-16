using System.Diagnostics.CodeAnalysis;
using CodeFirst.Utils.Logging;
using CodeFirst.Utils.Math;
using OpenTK.Mathematics;

namespace CodeFirst.Physics;

public class PhysicsWorld
{
	private static readonly ILogger Logger = LoggingManager.Get<PhysicsWorld>();

	private readonly HashSet<Guid> items;
	private readonly Dictionary<Guid, Box2> bodies;

	/// <summary>
	///     Used as persistent storage of already checked items when performing iterative collision resolution
	/// </summary>
	private readonly HashSet<Guid> alreadyChecked;

	public PhysicsWorld()
	{
		items = new HashSet<Guid>();
		bodies = new Dictionary<Guid, Box2>();
		alreadyChecked = new HashSet<Guid>();
	}

	public Guid Create(float x, float y, float width, float height) =>
		Create(new Box2(x, y, x + width, y + height));

	public Guid Create(Vector2 position, Vector2 size) => Create(Box2.FromSize(position, size));

	public Guid Create(Box2 body)
	{
		var newItem = Guid.NewGuid();
		items.Add(newItem);
		bodies.Add(newItem, body);
		Logger.Trace($"Created item {newItem} with body {body}.");
		return newItem;
	}

	/// <summary>
	///     Moves this body to the new position. If any collision occurs at the new position the collision(s) will be
	///     resolved and the body's position will be adjusted to not overlap any other bodies. For fast-moving bodies
	///     it can be advisable to call <see cref="Sweep" /> instead since <see cref="Move" /> will not check for collisions
	///     between the body's original position and its new position, only at the destination.
	/// </summary>
	/// <param name="item">the item to move</param>
	/// <param name="destination">the new position of the body</param>
	/// <param name="filterFunction"></param>
	/// <returns>the result of this move</returns>
	/// <seealso cref="Sweep" />
	/// <seealso cref="Place" />
	public CollisionResult Move(Guid item, Vector2 destination, CollisionFilterFunction? filterFunction = null)
	{
		if (!items.Contains(item))
		{
			throw new ArgumentException($"Item {item} was not created by this PhysicsWorld!", nameof(item));
		}

		filterFunction ??= DefaultCollisionFunction;
		alreadyChecked.Clear();
		alreadyChecked.Add(item);

		var body = bodies[item];
		body.Translate(destination - body.Location); // Move to destination

		var collisions = new List<Collision>();
		while (CheckCollision(item, body, filterFunction, out var collision))
		{
			collisions.Add(collision);
			alreadyChecked.Add(collision.Other);

			switch (collision.Resolution)
			{
				case CollisionResolution.Slide:
				{
					// Slide along the smallest direction
					var (slideDirection, separationDirection) =
						MathF.Abs(collision.Normal.X) > MathF.Abs(collision.Normal.Y)
							? (Vector2.UnitY, Vector2.UnitX)
							: (Vector2.UnitX, Vector2.UnitY);
					var slideVector = Project(collision.Separation, slideDirection);
					var separationVector = Project(collision.Separation, separationDirection);
					body.Translate(separationVector + slideVector);
					break;
				}
				case CollisionResolution.Touch:
				{
					body.Translate(collision.Separation);
					break;
				}
				case CollisionResolution.Bounce:
				{
					var separationVector = collision.Separation;
					// Mirror along the smallest direction
					if (MathF.Abs(collision.Normal.X) > MathF.Abs(collision.Normal.Y))
					{
						separationVector.Y *= -1;
					}
					else
					{
						separationVector.X *= -1;
					}

					body.Translate(separationVector);
					break;
				}
				case CollisionResolution.Cross: // ! Fallthrough
				case CollisionResolution.None: // ! Fallthrough
				default:
					break; // No need to resolve these collisions
			}
		}

		bodies[item] = body;
		return new CollisionResult(body.Location, collisions);
	}

	/// <summary>
	///     Moves this body to the new position in a "sweeping" manner, reliably detecting all collisions along its path.
	///     This is slower than moving a body normally but will prevent tunneling which makes this suitable for fast-moving
	///     bodies such as bullets or other projectiles.
	/// </summary>
	/// <param name="item">the item to sweep</param>
	/// <param name="destination">the new position of the body</param>
	/// <param name="filterFunction"></param>
	/// <returns>the result of this sweep</returns>
	/// <seealso cref="Move" />
	/// <seealso cref="Place" />
	public CollisionResult Sweep(Guid item, Vector2 destination, CollisionFilterFunction? filterFunction = null)
	{
		if (!items.Contains(item))
		{
			throw new ArgumentException($"Item {item} was not created by this PhysicsWorld!", nameof(item));
		}

		filterFunction ??= DefaultCollisionFunction;
		alreadyChecked.Clear();
		alreadyChecked.Add(item);

		var body = bodies[item];
		var velocity = destination - body.Location;

		var collisions = new List<Collision>();
		while (CheckSweptCollision(item, body, velocity, filterFunction, out var collision))
		{
			collisions.Add(collision);
			alreadyChecked.Add(collision.Other);

			switch (collision.Resolution)
			{
				case CollisionResolution.Slide:
				{
					// Move to collision location
					body.Translate(velocity * (1f - collision.Theta));
					var slideVector = CalculateSlideVector(collision.Normal, velocity);
					body.Translate(Project(velocity, slideVector) * collision.Theta);
					// Slide along the smallest direction (also, SIC! on the DOT product calculation)
					// var slideDot = velocity.X * collision.Normal.X + velocity.Y * collision.Normal.Y;
					// body.Translate(collision.Normal * slideDot * collision.Theta);
					break;
				}
				case CollisionResolution.Touch:
				{
					body.Translate(velocity * (1f - collision.Theta));
					break;
				}
				case CollisionResolution.Bounce:
				{
					// TODO
					break;
				}
				case CollisionResolution.Cross: // ! Fallthrough
				case CollisionResolution.None: // ! Fallthrough
				default:
					break; // No need to resolve these collisions
			}
		}

		if (collisions.Count == 0)
		{
			body.Translate(velocity);
		}

		bodies[item] = body;
		return new CollisionResult(body.Location, collisions);
	}

	/// <summary>
	///     Places the body at the given position without checking for collisions in between. Use this to teleport objects.
	///     By default this will report all collisions that occured at the new position but will <b>NOT</b> resolve
	///     those collisions (i.e. treated as <see cref="CollisionResolution.Cross">CollisionResolution.Cross</see>.
	///     If you wish to resolve this body to be moved outside of any colliding bodies use <see cref="Move" /> or
	///     <see cref="Sweep" /> instead.
	/// </summary>
	/// <param name="item">the item to place</param>
	/// <param name="destination">the new position of the body</param>
	/// <param name="filterFunction"></param>
	/// <returns>the result of this place</returns>
	/// <seealso cref="Move" />
	/// <seealso cref="Sweep" />
	public CollisionResult Place(Guid item, Vector2 destination, CollisionFilterFunction? filterFunction = null)
	{
		if (!items.Contains(item))
		{
			throw new ArgumentException($"Item {item} was not created by this PhysicsWorld!", nameof(item));
		}

		filterFunction ??= DefaultCollisionFunction;

		var body = bodies[item];
		body.Translate(destination - body.Location); // Move to destination
		bodies[item] = body;

		var collisions = new List<Collision>();
		foreach (var otherItem in items.Except(alreadyChecked).Where(otherItem => otherItem != item))
		{
			var otherBody = bodies[otherItem];
			if (!Collides(body, otherBody, out var normal, out var theta))
			{
				continue;
			}

			var resolution = filterFunction(item, otherItem);
			if (resolution == CollisionResolution.None)
			{
				continue;
			}

			collisions.Add(new Collision(item, otherItem, resolution, normal, theta));
		}

		return new CollisionResult(destination, collisions);
	}

	private bool CheckCollision(Guid item, Box2 body, CollisionFilterFunction filterFunction,
		[NotNullWhen(true)] out Collision? collision)
	{
		collision = null;
		foreach (var otherItem in items.Except(alreadyChecked).Where(otherItem => otherItem != item))
		{
			var otherBody = bodies[otherItem];
			if (!Collides(body, otherBody, out var normal, out var theta))
			{
				continue;
			}

			var resolution = filterFunction(item, otherItem);
			if (resolution == CollisionResolution.None)
			{
				continue;
			}

			if (collision == null)
			{
				collision = new Collision(item, otherItem, resolution, normal, theta);
				break;
			}

			if (theta > collision.Theta)
			{
				collision = new Collision(item, otherItem, resolution, normal, theta);
			}
		}

		return collision != null;
	}

	private bool CheckSweptCollision(Guid item, Box2 body, Vector2 velocity, CollisionFilterFunction filterFunction,
		[NotNullWhen(true)] out Collision? collision)
	{
		collision = null;
		var broadphaseRectangle = CreateBroadphaseRectangle(
			body.X, body.Y,
			body.X + velocity.X, body.Y + velocity.Y,
			body.Width, body.Height
		);
		foreach (var otherItem in items.Except(alreadyChecked).Where(otherItem => otherItem != item))
		{
			var otherBody = bodies[otherItem];
			if (!otherBody.IntersectsWith(broadphaseRectangle))
			{
				continue;
			}

			var resolution = filterFunction(item, otherItem);
			if (resolution == CollisionResolution.None)
			{
				continue;
			}
			

			if (!SweepCollides(body, otherBody, velocity, out var normal, out var theta))
			{
				continue;
			}

			if (collision == null)
			{
				collision = new Collision(item, otherItem, resolution, normal, theta);
				break;
			}

			if (theta > collision.Theta)
			{
				collision = new Collision(item, otherItem, resolution, normal, theta);
			}
		}

		return collision != null;
	}

	private static Box2 CreateBroadphaseRectangle(
		float oldX, float oldY,
		float newX, float newY,
		float width, float height)
		=> Box2.FromPositions(
			new Vector2(MathF.Min(oldX, newX), MathF.Min(oldY, newY)),
			new Vector2(MathF.Max(oldX, newX) + width, MathF.Max(oldY, newY) + height)
		);

	private static bool Collides(Box2 firstAabb, Box2 secondAabb, out Vector2 normal, out float theta)
	{
		var minimumTranslationDistance = float.MaxValue;
		var minimumTranslationAxis = Vector2.Zero;

		if (!TestSeparatingAxis(Vector2.UnitX, firstAabb.Min.X, firstAabb.Max.X, secondAabb.Min.X, secondAabb.Max.X,
			    ref minimumTranslationAxis, ref minimumTranslationDistance))
		{
			normal = default;
			theta = default;
			return false;
		}

		if (!TestSeparatingAxis(Vector2.UnitY, firstAabb.Min.Y, firstAabb.Max.Y, secondAabb.Min.Y, secondAabb.Max.Y,
			    ref minimumTranslationAxis, ref minimumTranslationDistance))
		{
			normal = default;
			theta = default;
			return false;
		}

		minimumTranslationAxis.Normalize();
		normal = minimumTranslationAxis;
		theta = MathF.Sqrt(minimumTranslationDistance);
		return true;
	}

	private static bool SweepCollides(Box2 first, Box2 second, Vector2 velocity, out Vector2 normal, out float theta)
	{
		float xInvEntry;
		float yInvEntry;
		float xInvExit;
		float yInvExit;

		if (velocity.X > 0.0f)
		{
			xInvEntry = second.X - (first.X + first.Width);
			xInvExit = second.X + second.Width - first.X;
		}
		else
		{
			xInvEntry = second.X + second.Width - first.X;
			xInvExit = second.X - (first.X + first.Width);
		}

		if (velocity.Y > 0.0f)
		{
			yInvEntry = second.Y - (first.Y + first.Height);
			yInvExit = second.Y + second.Height - first.Y;
		}
		else
		{
			yInvEntry = second.Y + second.Height - first.Y;
			yInvExit = second.Y - (first.Y + first.Height);
		}

		float xEntry;
		float yEntry;
		float xExit;
		float yExit;

		if (velocity.X == 0f)
		{
			xEntry = float.NegativeInfinity;
			xExit = float.PositiveInfinity;
		}
		else
		{
			xEntry = xInvEntry / velocity.X;
			xExit = xInvExit / velocity.X;
		}

		if (velocity.Y == 0.0f)
		{
			yEntry = float.NegativeInfinity;
			yExit = float.PositiveInfinity;
		}
		else
		{
			yEntry = yInvEntry / velocity.Y;
			yExit = yInvExit / velocity.Y;
		}

		// if (yEntry > 1.0f) yEntry = float.NegativeInfinity;
		// if (xEntry > 1.0f) xEntry = float.NegativeInfinity;

		var entryTime = MathF.Max(xEntry, yEntry);
		var exitTime = MathF.Min(xExit, yExit);

		if (entryTime > exitTime || (xEntry < 0.0f && yEntry < 0.0f) || xEntry > 1f || yEntry > 1f)
		{
			// No collision
			normal = Vector2.Zero;
			theta = 1f;
			return false;
		}

		if (xEntry > yEntry)
		{
			normal = xInvEntry < 0.0f ? Vector2.UnitX : -Vector2.UnitX;
		}
		else
		{
			normal = yInvEntry < 0.0f ? Vector2.UnitY : -Vector2.UnitY;
		}

		// TODO: Figure out if we want this to be 1 - entryTime or just entryTime
		theta = 1f - entryTime;
		return true;
	}

	private static bool TestSeparatingAxis(Vector2 referenceAxis, float firstMin, float firstMax, float secondMin,
		float secondMax,
		ref Vector2 minimumTranslationAxis, ref float minimumTranslationDistance)
	{
		// Implementation of separating axis theorem
		var axisLengthSquared = Vector2.Dot(referenceAxis, referenceAxis);
		if (axisLengthSquared < float.Epsilon)
		{
			// Axis is invalid
			return true;
		}

		// Check overlap on the left and right sides
		var leftDistance = secondMax - firstMin;
		var rightDistance = firstMax - secondMin;

		// No overlap
		if (leftDistance <= 0.0f || rightDistance <= 0.0f)
		{
			return false;
		}

		// Find out which side we overlap on
		var overlap = leftDistance < rightDistance ? leftDistance : -rightDistance;
		var separationVector = referenceAxis * (overlap / axisLengthSquared);
		var separationLengthSquared = Vector2.Dot(separationVector, separationVector);
		if (separationLengthSquared < minimumTranslationDistance)
		{
			minimumTranslationDistance = separationLengthSquared;
			minimumTranslationAxis = separationVector;
		}

		return true;
	}

	private static CollisionResolution DefaultCollisionFunction(Guid _, Guid __) => CollisionResolution.Slide;

	private static Vector2 CalculateSlideVector(Vector2 normal, Vector2 velocity)
	{
		if (MathF.Abs(normal.X) > MathF.Abs(normal.Y))
		{
			// Pointing in X direction, slide is in Y direction
			return velocity.Y > 0 ? Vector2.UnitY : -Vector2.UnitY;
		}

		// Pointing in Y direction, slide is in X direction
		return velocity.X > 0 ? Vector2.UnitX : -Vector2.UnitX;
	}

	private static Vector2 Project(Vector2 source, Vector2 normal)
	{
		var lengthSquared = Vector2.Dot(normal, normal);
		if (lengthSquared <= 0)
		{
			return Vector2.Zero;
		}

		return normal * Vector2.Dot(source, normal) / lengthSquared;
	}

	private static Box2 CalculateMinkowskiDifference(Box2 first, Box2 second)
	{
		var location = first.Min - second.Max;
		var fullSize = first.Size + second.Size;
		return Box2.FromSize(location, fullSize);
	}
}

public record CollisionResult(Vector2 Position, List<Collision> Collisions)
{
	public static readonly CollisionResult Default = new(Vector2.Zero, new List<Collision>());
}