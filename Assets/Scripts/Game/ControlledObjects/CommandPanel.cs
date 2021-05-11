
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
		if (m_TriggerCollider == null )
		{
			Debug.LogError( "CommandPanel " + name + " has not TriggerCollider child" );
		}

		m_TriggerCollider.isTrigger = true;

		if (m_ObjectToControl == null || m_Activator == null )
		{
			Debug.LogError( "CommandPanel " + name + " has not ObjectToControl and Activator set!!" );
			m_TriggerCollider.enabled = false;
			return;
		}

		CustomAssertions.IsNotNull(GameManager.SaveAndLoad);

		GameManager.SaveAndLoad.OnSave += OnSave;
		GameManager.SaveAndLoad.OnLoad += OnLoad;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if (GameManager.SaveAndLoad.IsNotNull())
		{
			GameManager.SaveAndLoad.OnSave -= OnSave;
			GameManager.SaveAndLoad.OnLoad -= OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit OnSave(StreamData streamData)
	{
		StreamUnit streamUnit = streamData.NewUnit(gameObject);

		streamUnit.SetInternal("IsTriggered", m_IsTriggered);

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	private StreamUnit OnLoad(StreamData streamData)
	{
		bool bResult = streamData.TryGetUnit(gameObject, out StreamUnit streamUnit);
		if (bResult)
		{
			if (m_IsTriggered = streamUnit.GetAsBool( "IsTriggered" ))
			{
				m_ObjectToControl.OnActivation();
				m_Activator.transform.position			= m_TriggerCollider.transform.position;
				m_Activator.transform.rotation			= m_TriggerCollider.transform.rotation;
				m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
				m_Activator.RigidBody.useGravity		= false;
				m_Activator.Collider.enabled			= false;
				m_TriggerCollider.enabled				= false;
			}
		}
		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other.GetInstanceID() == m_Activator.Collider.GetInstanceID() )
		{
			Player.Instance.Interactions.DropGrabbedObject();

			m_ObjectToControl.OnActivation();
			m_IsTriggered = true;

			m_Activator.transform.position			= m_TriggerCollider.transform.position;
			m_Activator.transform.rotation			= m_TriggerCollider.transform.rotation;
			m_Activator.RigidBody.constraints		= RigidbodyConstraints.FreezeAll;
			m_Activator.RigidBody.useGravity		= false;
			m_Activator.Collider.enabled			= false;
			m_TriggerCollider.enabled				= false;
		}
	}

}
