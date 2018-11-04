using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

public class PathWaypoint : MonoBehaviour {

	private	Path	m_Path	= null;

	private void Awake()
	{
		if( ( m_Path = GetComponentInParent<Path>() ) == null )
		{
			this.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		m_Path.DraawGizmos();
	}

}
