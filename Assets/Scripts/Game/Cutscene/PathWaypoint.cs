using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

public class PathWaypoint : MonoBehaviour {

	private void OnDrawGizmosSelected()
	{
		SplinePath path = GetComponentInParent<SplinePath>();
		path.DrawGizmos();
	}

}
