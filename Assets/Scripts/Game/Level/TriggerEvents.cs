
using UnityEngine;


public class TriggerEvents : MonoBehaviour {
	
	[SerializeField]
	private		GameEvent		m_OnEnter				= null;

	[SerializeField]
	private		GameEvent		m_OnExit				= null;

	[SerializeField]
	private		bool			m_TriggerOnce			= false;

	[SerializeField]
	private		bool			m_BypassEntityCheck		= false;


	private		bool			m_HasTriggered			= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( override )
	private StreamingUnit OnSave( StreamingData streamingData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		StreamingUnit streamingUnit		= streamingData.NewUnit( gameObject );
		streamingUnit.AddInternal( "HasTriggered", m_HasTriggered );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( override )
	private StreamingUnit OnLoad( StreamingData streamingData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		// Get unit
		StreamingUnit streamingUnit = null;
		if ( streamingData.GetUnit( gameObject, ref streamingUnit ) == false )
		{
			return null;
		}
		
		// TRIGGERED
		m_HasTriggered = streamingUnit.GetAsBool( "HasTriggered" );
		
		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
			return;

		if ( m_BypassEntityCheck == false && Player.Entity.CanTrigger() == false )
			return;

		m_HasTriggered = true;

		if ( m_OnEnter != null && m_OnEnter.GetPersistentEventCount() > 0 )
		{
			m_OnEnter.Invoke();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if ( m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
			return;

		if ( m_OnExit != null && m_OnExit.GetPersistentEventCount() > 0 )
		{
			m_OnExit.Invoke();
		}
	}

}
