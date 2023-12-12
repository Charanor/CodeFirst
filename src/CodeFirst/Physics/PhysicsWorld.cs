using CodeFirst.Utils;
using CodeFirst.Utils.Logging;
using CodeFirst.Utils.Math;
using OpenTK.Mathematics;

namespace CodeFirst.Physics;

public class PhysicsWorld
{
	private static readonly ILogger Logger = LoggingManager.Get<PhysicsWorld>();

	private readonly HashSet<Guid> items;
	private readonly Dictionary<Guid, Box2> bodies;
	private readonly QuadTree quadTree;
	private readonly Dictionary<Guid, int> quadTreeIds;
	private readonly Dictionary<Guid, int> quadTreeElements;

	private readonly Queue<int> reclaimedIds;
	private int nextId;

	public PhysicsWorld(int width = short.MaxValue, int height = short.MinValue, int initialElementCount = 8192,
		int initialMaxDepth = 3)
	{
		items = new HashSet<Guid>();
		bodies = new Dictionary<Guid, Box2>();
		quadTree = new QuadTree(width, height, initialElementCount, initialMaxDepth);
		quadTreeIds = new Dictionary<Guid, int>();
		quadTreeElements = new Dictionary<Guid, int>();

		reclaimedIds = new Queue<int>();
	}

	public Guid Create(float x, float y, float width, float height) =>
		Create(new Box2(x - width / 2f, y - height / 2f, x + width / 2f, y + height / 2f));

	public Guid Create(Vector2 position, Vector2 size) => Create(Box2.FromSize(position - size / 2f, size));

	public Guid Create(Box2 body)
	{
		var newItem = Guid.NewGuid();
		items.Add(newItem);
		bodies.Add(newItem, body);

		var id = GetNextQuadTreeId();
		quadTreeIds.Add(newItem, id);
		quadTreeElements.Add(newItem, quadTree.Insert(id, body.Left, body.Top, body.Right, body.Bottom));

		return newItem;
	}

	private int GetNextQuadTreeId()
	{
		if (reclaimedIds.Count > 0)
		{
			return reclaimedIds.Dequeue();
		}

		var id = nextId;
		nextId += 1;
		return id;
	}

	public void Destroy(Guid item)
	{
		if (!items.Contains(item))
		{
			DevTools.Throw<PhysicsWorld>(new ArgumentException($"Item {item} was not created by this PhysicsWorld!",
				nameof(item)));
			return;
		}

		items.Remove(item);
		bodies.Remove(item);
		if (quadTreeIds.Remove(item, out var id))
		{
			reclaimedIds.Enqueue(id);
		}

		if (quadTreeElements.Remove(item, out var element))
		{
			quadTree.Remove(element);
		}
	}

	/// <summary>
	///     Moves this body to the new position, detecting collisions along its path.
	/// </summary>
	/// <param name="item">the item to move</param>
	/// <param name="destination">the new position of the body</param>
	/// <param name="filterFunction"></param>
	/// <returns>the result of this move</returns>
	/// <seealso cref="Place" />
	public CollisionResult Move(Guid item, Vector2 destination, CollisionFilterFunction? filterFunction = null)
	{
		if (!items.Contains(item))
		{
			DevTools.Throw<PhysicsWorld>(new ArgumentException($"Item {item} was not created by this PhysicsWorld!",
				nameof(item)));
			return CollisionResult.Default;
		}

		filterFunction ??= DefaultCollisionFunction;

		var body = bodies[item];
		var collisions = new List<Collision>();
		while (true)
		{
			var velocity = destination - body.Center;
			var broadphaseRectangle = CreateBroadphaseRectangle(
				body.X, body.Y,
				destination.X - body.Width / 2f, destination.Y - body.Height / 2f,
				body.Width, body.Height
			);

			var hasCollision = false;
			var closestTheta = 1f;
			var closestNormal = Vector2.Zero;
			var closestResolution = CollisionResolution.None;
			Guid closestGuid = default;
			
			foreach (var other in items)
			{
				if (other == item)
				{
					continue;
				}

				var otherBody = bodies[other];

				// Broadphase
				if (!broadphaseRectangle.IntersectsWith(otherBody))
				{
					continue;
				}

				// Narrow phase
				if (!SweepCollides(body, otherBody, velocity, out var normal, out var theta))
				{
					continue;
				}

				// This collision was not closer
				if (theta >= closestTheta)
				{
					continue;
				}

				var resolution = filterFunction(item, other);
				if (resolution == CollisionResolution.None)
				{
					// No resolution to this collision
					continue;
				}

				hasCollision = true;
				closestTheta = theta;
				closestNormal = normal;
				closestResolution = resolution;
				closestGuid = other;
			}

			// Move body all the way until it collides
			// Also move body away from wall very slightly to prevent getting stuck on seams
			const float seamMargin = 0.001f;
			destination = body.Center + velocity * closestTheta + closestNormal * seamMargin;

			if (!hasCollision)
			{
				// No collision, our work here is done
				break;
			}

			var normalLengthSquared = Vector2.Dot(closestNormal, closestNormal);
			if (normalLengthSquared == 0)
			{
				// No normal? :/
				continue;
			}

			collisions.Add(new Collision(item, closestGuid, closestResolution, closestNormal, closestTheta));

			var resolutionTheta = 1 - closestTheta;
			switch (closestResolution)
			{
				case CollisionResolution.Slide:
				{
					var slideDot = resolutionTheta * Vector2.Dot(velocity, closestNormal);
					var slideDirection = resolutionTheta * velocity - slideDot / normalLengthSquared * closestNormal;
					destination += slideDirection;
					break;
				}
				case CollisionResolution.Touch:
					// Touch is already handled above, basically touch means only separate and nothing else
					break;
				case CollisionResolution.Bounce:
				{
					var bounceNormal = velocity.Normalized();
					if (closestNormal.X != 0)
					{
						// Normal is X-aligned, so reflect on Y-axis
						bounceNormal.Y *= -1;
					}
					else if (closestNormal.Y != 0)
					{
						// Normal is Y-aligned, so reflect on X-axis
						bounceNormal.X *= -1;
					}

					destination += bounceNormal * resolutionTheta;
					break;
				}
				case CollisionResolution.Cross:
					// No separation, only record collision
					break;
				case CollisionResolution.None: // Should never happen
				default:
					DevTools.Throw<PhysicsWorld>(new InvalidOperationException(
						$"Unexpected resolution '{nameof(CollisionResolution.None)}' during resolution! This should be handled in a special case before."));
					break;
			}
		}

		body.Center = destination;
		bodies[item] = body;
		return new CollisionResult(body.Center, collisions);
	}

	/// <summary>
	///     Places the body at the given position without checking for collisions in between. Use this to teleport objects.
	///     By default this will report all collisions that occured at the new position but will <b>NOT</b> resolve
	///     those collisions (i.e. treated as <see cref="CollisionResolution.Cross">CollisionResolution.Cross</see>.
	///     If you wish to resolve this body to be moved outside of any colliding bodies use <see cref="Move" /> instead.
	/// </summary>
	/// <param name="item">the item to place</param>
	/// <param name="destination">the new position of the body</param>
	/// <param name="filterFunction"></param>
	/// <returns>the result of this place</returns>
	/// <seealso cref="Move" />
	public CollisionResult Place(Guid item, Vector2 destination, CollisionFilterFunction? filterFunction = null)
	{
		if (!items.Contains(item))
		{
			DevTools.Throw<PhysicsWorld>(new ArgumentException($"Item {item} was not created by this PhysicsWorld!",
				nameof(item)));
			return CollisionResult.Default;
		}

		filterFunction ??= DefaultCollisionFunction;

		var body = bodies[item];
		body.Center = destination;
		bodies[item] = body;

		var collisions = new List<Collision>();
		foreach (var otherItem in items)
		{
			if (otherItem == item)
			{
				continue;
			}

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

	public Box2 GetBody(Guid item) => items.Contains(item) ? bodies[item] : Box2.Empty;

	public bool Raycast(in Ray2D ray, List<RaycastHit> hits)
	{
		hits.Clear();

		foreach (var (otherItem, otherBody) in bodies)
		{
			if (!Intersect.RayAabb(in ray, in otherBody, out var distance))
			{
				continue;
			}

			hits.Add(new RaycastHit(otherItem, distance));
		}

		hits.Sort((first, second) => Comparer<float>.Default.Compare(first.Distance, second.Distance));
		return hits.Count > 0;
	}

	public bool Raycast(in Ray2D ray, out List<RaycastHit> hits)
	{
		hits = new List<RaycastHit>();
		return Raycast(in ray, hits);
	}

	public bool Raycast(in Ray2D ray, out RaycastHit firstHit)
	{
		var hasHit = false;
		Guid nearestItem = default;
		var nearestDistance = float.PositiveInfinity;

		foreach (var (otherItem, otherBody) in bodies)
		{
			if (!Intersect.RayAabb(in ray, in otherBody, out var distance))
			{
				continue;
			}

			if (distance >= nearestDistance)
			{
				continue;
			}

			nearestItem = otherItem;
			nearestDistance = distance;
			hasHit = true;
		}

		if (!hasHit)
		{
			firstHit = default;
			return false;
		}

		firstHit = new RaycastHit(nearestItem, nearestDistance);
		return true;
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
		var combinedX = second.X - (first.X + first.SizeX);
		var combinedY = second.Y - (first.Y + first.SizeY);
		var combinedSizeX = first.SizeX + second.SizeX;
		var combinedSizeY = first.SizeY + second.SizeY;
		var (velocityX, velocityY) = velocity;

		// Original cond
		theta = 1;
		normal = Vector2.Zero;

		var xMinDist = LineToPlane(velocityX, velocityY, combinedX, combinedY, nx: -1, ny: 0);
		if (xMinDist >= 0 && velocityX > 0 && xMinDist < theta &&
		    Between(xMinDist * velocityY, combinedY, combinedY + combinedSizeY))
		{
			theta = xMinDist;
			normal = -Vector2.UnitX;
		}

		var xMaxDist = LineToPlane(velocityX, velocityY, combinedX + combinedSizeX, combinedY, nx: 1, ny: 0);
		if (xMaxDist >= 0 && velocityX < 0 && xMaxDist < theta &&
		    Between(xMaxDist * velocityY, combinedY, combinedY + combinedSizeY))
		{
			theta = xMaxDist;
			normal = Vector2.UnitX;
		}

		var yMinDist = LineToPlane(velocityX, velocityY, combinedX, combinedY, nx: 0, ny: -1);
		if (yMinDist >= 0 && velocityY > 0 && yMinDist < theta &&
		    Between(yMinDist * velocityX, combinedX, combinedX + combinedSizeX))
		{
			theta = yMinDist;
			normal = -Vector2.UnitY;
		}

		var yMaxDist = LineToPlane(velocityX, velocityY, combinedX, combinedY + combinedSizeY, nx: 0, ny: 1);
		if (yMaxDist >= 0 && velocityY < 0 && yMaxDist < theta &&
		    Between(yMaxDist * velocityX, combinedX, combinedX + combinedSizeX))
		{
			theta = yMaxDist;
			normal = Vector2.UnitY;
		}

		return theta is > 0 and < 1;
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

	private static float LineToPlane(float ux, float uy, float vx, float vy, float nx, float ny)
	{
		var normalDotPosition = nx * ux + ny * uy;
		if (normalDotPosition == 0)
		{
			return float.PositiveInfinity;
		}

		return (nx * vx + ny * vy) / normalDotPosition;
	}

	private static bool Between(float x, float a, float b) => x >= a && x <= b;
}