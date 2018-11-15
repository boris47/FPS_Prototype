using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CutScene;

[System.Serializable]
public class PathWaypoint : MonoBehaviour {

	public	SimMovementType					movementType		= SimMovementType.WALK;
	public	Transform						point				= null;
	public	Transform						target				= null;
	[Range( 0.01f, 1f )]
	public	float							timeScaleTraget		= 1.0f;
	public	bool							zoomEnabled			= false;
	public	Cutscene_Waiter_Base			waiter				= null;

	[SerializeField]
	public	GameEvent						OnWayPointReached	= null;

	public	void	OnReached()
	{
		if ( OnWayPointReached != null && OnWayPointReached.GetPersistentEventCount() > 0 )
		{
			OnWayPointReached.Invoke();
		}
	}

	private void OnDrawGizmosSelected()
	{
		PathSpline path = GetComponentInParent<PathSpline>();
		path.DrawGizmos();
	}

}
