
using UnityEngine;

public class CommandPanel : MonoBehaviour {

	[SerializeField]
	private		ObjectActivator			m_Activator				= null;

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
	private	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit	= streamingData.NewUnit( gameObject );

		streamingUnit.AddInternal( "IsTriggered", m_IsTriggered );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = null;
		if ( streamingData.GetUnit( gameObject, ref streamingUnit ) == false )
			return null;

		m_IsTriggered = streamingUnit.GetAsBool( "IsTriggered" );

		if ( m_IsTriggered == true )
		{
			m_ObjectToControl.OnActivation();
			m_Activator.transform.position		= m_TriggerZone.transform.position;
			m_Activator.transform.rotation		= m_TriggerZone.transform.rotation;
			m_Activator.Rigidbody.constraints	= RigidbodyConstraints.FreezeAll;
			m_Activator.Rigidbody.useGravity	= false;
			m_Activator.Collider.enabled		= false;
			m_TriggerCollider.enabled			= false;
		}

		return streamingUnit;
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

			m_Activator.transform.position		= m_TriggerZone.transform.position;
			m_Activator.transform.rotation		= m_TriggerZone.transform.rotation;
			m_Activator.Rigidbody.constraints	= RigidbodyConstraints.FreezeAll;
			m_Activator.Rigidbody.useGravity	= false;
			m_Activator.Collider.enabled		= false;
			m_TriggerCollider.enabled			= false;
		}
	}

}
