using UnityEngine;
using System.Collections.Generic;



public abstract class IterableVolume : VolumeBase
{
	//////////////////////////////////////////////////////////////////////////
	public abstract void IterateOver(System.Action<Vector3, Vector3> OnPosition, in float InMargin = 0f,
		in float? InCountOnX = null, in float? InCountOnY = null, in float? InCountOnZ = null);


	//////////////////////////////////////////////////////////////////////////
	public Vector3[] GetPoints()
	{
		List<Vector3> points = new List<Vector3>();
		IterateOver((p, s) => points.Add(p));
		return points.ToArray();
	}

	//////////////////////////////////////////////////////////////////////////
	public (Vector3, Vector3)[] GetPointsAndSize()
	{
		List<(Vector3, Vector3)> points = new List<(Vector3, Vector3)>();
		IterateOver((p, s) => points.Add((p, s)));
		return points.ToArray();
	}

}
