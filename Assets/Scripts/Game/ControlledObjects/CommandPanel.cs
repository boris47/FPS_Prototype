﻿
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
	}

	private void OnEnable()
	{
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
	}

	private void OnDisable()
	{
		if (GameManager.StreamEvents.IsNotNull())
		{
			GameManager.StreamEvents.OnSave	-= OnSave;
			GameManager.StreamEvents.OnLoad	-= OnLoad;	
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	bool	OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		streamUnit = streamData.NewUnit(gameObject );

		streamUnit.SetInternal( "IsTriggered", m_IsTriggered );

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	bool	OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit );
		if (bResult)
		{
			if (m_IsTriggered = streamUnit.GetAsBool( "IsTriggered" ) == true )
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
		return bResult;
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
