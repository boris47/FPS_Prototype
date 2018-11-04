using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

public class PathWaypoint : MonoBehaviour {

	private void OnDrawGizmosSelected()
	{
		Path path = GetComponentInParent<Path>();
		path.DraawGizmos();
	}

}
