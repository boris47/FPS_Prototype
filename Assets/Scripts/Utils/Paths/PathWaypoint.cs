using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

public class PathWaypoint : MonoBehaviour {

	[SerializeField]
	private	GameEvent	m_OnWaipointReached	= null;


	public	void	OnReached()
	{
		if ( m_OnWaipointReached != null && m_OnWaipointReached.GetPersistentEventCount() > 0 )
		{
			m_OnWaipointReached.Invoke();
		}
	}

	private void OnDrawGizmosSelected()
	{
		SplinePath path = GetComponentInParent<SplinePath>();
		path.DrawGizmos();
	}

}
