using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace CodeFirst.Utils.Math;

public readonly record struct Frustum(Plane Near, Plane Far, Plane Left, Plane Right, Plane Top, Plane Bottom)
{
	private IReadOnlyList<Plane> Planes { get; } = new[] { Near, Far, Left, Right, Top, Bottom };

	[Pure]
	public bool IntersectsAabb(Box3 aabb)
	{
		// These are just the corners of the aabb
		var leftBottomFront = new Vector3(aabb.Left, aabb.Bottom, aabb.Front);
		var rightBottomFront = new Vector3(aabb.Right, aabb.Bottom, aabb.Front);
		var leftBottomBack = new Vector3(aabb.Left, aabb.Bottom, aabb.Back);
		var rightBottomBack = new Vector3(aabb.Right, aabb.Bottom, aabb.Back);
		var leftTopFront = new Vector3(aabb.Left, aabb.Top, aabb.Front);
		var rightTopFront = new Vector3(aabb.Right, aabb.Top, aabb.Front);
		var leftTopBack = new Vector3(aabb.Left, aabb.Top, aabb.Back);
		var rightTopBack = new Vector3(aabb.Right, aabb.Top, aabb.Back);
	
		// ReSharper disable once ForCanBeConvertedToForeach
		// ReSharper disable once LoopCanBeConvertedToQuery
		for (var i = 0; i < Planes.Count; i++)
		{
			var plane = Planes[i];
			if (plane.DistanceTo(leftBottomFront) < 0 &&
			    plane.DistanceTo(rightBottomFront) < 0 &&
			    plane.DistanceTo(leftBottomBack) < 0 &&
			    plane.DistanceTo(rightBottomBack) < 0 &&
			    plane.DistanceTo(leftTopFront) < 0 &&
			    plane.DistanceTo(rightTopFront) < 0 &&
			    plane.DistanceTo(leftTopBack) < 0 &&
			    plane.DistanceTo(rightTopBack) < 0)
			{
				return false;
			}
		}
	
		return true;
	}

	// public bool IntersectsAabb(Box3 aabb)
	// {
	// 	var b = new[] { aabb.Min, aabb.Max };
	//
	// 	var result = true;
	//
	// 	for (var i = 0; i < 6; ++i)
	// 	{
	// 		var px = Planes[i].A > 0.0f ? 1 : 0;
	// 		var py = Planes[i].B > 0.0f ? 1 : 0;
	// 		var pz = Planes[i].C > 0.0f ? 1 : 0;
	//
	// 		var dp = Planes[i].A * b[px].X + Planes[i].B * b[py].Y + Planes[i].C * b[pz].Z;
	// 		var dp2 = Planes[i].A * b[1 - px].X + Planes[i].B * b[1 - py].Y + Planes[i].C * b[1 - pz].Z;
	//
	// 		if (dp < -Planes[i].D)
	// 		{
	// 			return false;
	// 		}
	//
	// 		if (dp2 <= -Planes[i].D)
	// 		{
	// 			result = true;
	// 		}
	// 	}
	//
	// 	return result;
	// }

	[Pure]
	public bool IntersectsSphere(Vector3 center, float radius) =>
		// SIC! -radius
		Near.DistanceTo(center) > -radius ||
		Far.DistanceTo(center) > -radius ||
		Left.DistanceTo(center) > -radius ||
		Right.DistanceTo(center) > -radius ||
		Top.DistanceTo(center) > -radius ||
		Bottom.DistanceTo(center) > -radius;
}