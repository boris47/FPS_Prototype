
using UnityEngine;

public class CommandPanel : MonoBehaviour {

	[SerializeField]
	private		Interactable			m_Activator				= null;

	[SerializeField]
	private		ControlledObject		m_ObjectToControl		= null;

	[SerializeField]
	private		Collider				m_TriggerCollider		= null;
	private		bool					m_IsTriggered			= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		// Get Trigger Collider
		if (this.m_TriggerCollider == null )
		{
			Debug.LogError( "CommandPanel " + this.name + " has not TriggerCollider child" );
		}

		this.m_TriggerCollider.isTrigger = true;

		if (this.m_ObjectToControl == null || this.m_Activator == null )
		{
			Debug.LogError( "CommandPanel " + this.name + " has not ObjectToControl and Activator set!!" );
			this.m_TriggerCollider.enabled = false;
			return;
		}

		GameManager.StreamEvents.OnSave += this.OnSave;
		GameManager.StreamEvents.OnLoad += this.OnLoad;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );

		streamUnit.SetInternal( "IsTriggered", this.m_IsTriggered );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
			return null;

		if (this.m_IsTriggered = streamUnit.GetAsBool( "IsTriggered" ) == true )
		{
			this.m_ObjectToControl.OnActivation();
			this.m_Activator.transform.position			= this.m_TriggerCollider.transform.position;
			this.m_Activator.transform.rotation			= this.m_TriggerCollider.transform.rotation;
			this.m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
			this.m_Activator.RigidBody.useGravity		= false;
			this.m_Activator.Collider.enabled			= false;
			this.m_TriggerCollider.enabled				= false;
		}

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other.GetInstanceID() == this.m_Activator.Collider.GetInstanceID() )
		{
			Player.Instance.DropEntityDragged();

			this.m_ObjectToControl.OnActivation();
			this.m_IsTriggered = true;

			this.m_Activator.transform.position			= this.m_TriggerCollider.transform.position;
			this.m_Activator.transform.rotation			= this.m_TriggerCollider.transform.rotation;
			this.m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
			this.m_Activator.RigidBody.useGravity		= false;
			this.m_Activator.Collider.enabled			= false;
			this.m_TriggerCollider.enabled				= false;
		}
	}

}
