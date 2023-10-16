namespace CodeFirst.Physics;

public enum CollisionResolution
{
	/// <summary>
	///     No collisions will be checked for or reported.
	/// </summary>
	None,

	/// <summary>
	///     Colliding bodies will attempt to slide along other bodies.
	/// </summary>
	Slide,

	/// <summary>
	///     Colliding bodies will completely stop upon hitting another body.
	/// </summary>
	Touch,

	/// <summary>
	///     Colliding bodies will bounce off the other body along the reflection of its collision normal.
	/// </summary>
	Bounce,

	/// <summary>
	///     Collisions will be reported but no collision resolution will take place (used for triggers and the like).
	/// </summary>
	Cross
}