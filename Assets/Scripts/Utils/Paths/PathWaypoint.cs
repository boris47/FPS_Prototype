using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

[System.Serializable]
public class PathWaypoint : MonoBehaviour {

	[SerializeField]
	protected	Waiter_Base						m_Waiter			= null;

	[SerializeField]
	protected	GameEvent						m_OnWayPointReached	= null;



	//
	public	bool	HasToWait()
	{
		return (this.m_Waiter != null && this.m_Waiter.HasToWait );
	}


	//
	public	void	OnReached()
	{
		if (this.m_OnWayPointReached != null && this.m_OnWayPointReached.GetPersistentEventCount() > 0 )
		{
			this.m_OnWayPointReached.Invoke();
		}
	}


	//
	private void OnDrawGizmosSelected()
	{
		PathBase path = this.GetComponentInParent<PathBase>();
		path.DrawGizmos();
	}

}
