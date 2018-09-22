
using UnityEngine;

public class CommandPanel : MonoBehaviour {

	[SerializeField]
	private		Interactable			m_Activator				= null;

	[SerializeField]
	private		ControlledObject		m_ObjectToControl		= null;


	private		Transform				m_TriggerZone			= null;
	private		Collider				m_TriggerCollider		= null;
	private		bool					m_IsTriggered			= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		// Get Trigger Collider
		m_TriggerZone = transform.Find( "TriggerZone" );
		m_TriggerCollider = m_TriggerZone.GetComponent<Collider>();
		m_TriggerCollider.isTrigger = true;

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );

		streamUnit.AddInternal( "IsTriggered", m_IsTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
			return null;

		m_IsTriggered = streamUnit.GetAsBool( "IsTriggered" );

		if ( m_IsTriggered == true )
		{
			m_ObjectToControl.OnActivation();
			m_Activator.transform.position			= m_TriggerZone.transform.position;
			m_Activator.transform.rotation			= m_TriggerZone.transform.rotation;
			m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
			m_Activator.RigidBody.useGravity		= false;
			m_Activator.Collider.enabled			= false;
			m_TriggerCollider.enabled				= false;
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other == m_Activator.Collider )
		{
			Player.Instance.DropEntityDragged();

			m_ObjectToControl.OnActivation();
			m_IsTriggered = true;

			m_Activator.transform.position			= m_TriggerZone.transform.position;
			m_Activator.transform.rotation			= m_TriggerZone.transform.rotation;
			m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
			m_Activator.RigidBody.useGravity		= false;
			m_Activator.Collider.enabled			= false;
			m_TriggerCollider.enabled				= false;
		}
	}

}
