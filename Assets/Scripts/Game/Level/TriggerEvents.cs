
using UnityEngine;


public class TriggerEvents : MonoBehaviour {
	
	[SerializeField]
	private		GameEvent		m_OnEnter			= null;

	[SerializeField]
	private		GameEvent		m_OnExit			= null;


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
			return;

		if ( m_OnEnter != null && m_OnEnter.GetPersistentEventCount() > 0 )
		{
			m_OnEnter.Invoke();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
			return;

		if ( m_OnExit != null && m_OnExit.GetPersistentEventCount() > 0 )
		{
			m_OnExit.Invoke();
		}
	}

}
