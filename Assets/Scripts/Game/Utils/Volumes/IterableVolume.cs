using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


public interface IVolumeIterator
{
	void OnIterationStart();
	void OnIteration(Vector3 InPosition);
	void OnIterationCompleted();
}


public abstract class IterableVolume : MonoBehaviour
{
	//////////////////////////////////////////////////////////////////////////
	public abstract bool IterateOver(System.Action<Vector3> OnPosition);

	public Vector3[] GetPoints()
	{
		List<Vector3> points = new List<Vector3>();
		IterateOver(p => points.Add(p));
		return points.ToArray();
	}
}
