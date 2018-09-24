
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
		Collider collider = GetComponent<Collider>();
		if ( collider == null )
		{
			Destroy(this);
			return;
		}

		collider.isTrigger = true; // ensure is used as trigger

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave ( override )
	private StreamUnit OnSave( StreamData streamData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		StreamUnit streamUnit		= streamData.NewUnit( gameObject );
		streamUnit.AddInternal( "HasTriggered", m_HasTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( override )
	private StreamUnit OnLoad( StreamData streamData )
	{
		// Skip if no required
		if ( m_TriggerOnce == false )
			return null;

		// Get unit
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
		{
			return null;
		}
		
		// TRIGGERED
		m_HasTriggered = streamUnit.GetAsBool( "HasTriggered" );
		
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( m_TriggerOnce == true && m_HasTriggered == true )
			return;

		if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
			return;

		if ( m_BypassEntityCheck == false && Player.Instance.CanTrigger() == false )
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
