using UnityEngine;

[System.Serializable]
public class PathWaypoint : MonoBehaviour {

	[SerializeField]
	protected	Waiter_Base						m_Waiter			= null;

	[SerializeField]
	protected	GameEvent						m_OnWayPointReached	= null;



	//
	public	bool	HasToWait()
	{
		return (m_Waiter != null && m_Waiter.HasToWait );
	}


	//
	public	void	OnReached()
	{
		if (m_OnWayPointReached != null && m_OnWayPointReached.GetPersistentEventCount() > 0 )
		{
			m_OnWayPointReached.Invoke();
		}
	}


	//
	private void OnDrawGizmosSelected()
	{
		PathBase path = GetComponentInParent<PathBase>();
		path?.DrawGizmos();
	}

}
