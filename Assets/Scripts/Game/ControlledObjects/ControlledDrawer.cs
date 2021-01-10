// Scripted by Roberto Leogrande

using System.Collections;
using UnityEngine;

public class ControlledDrawer : ControlledObject {
	
	private	enum	EAxisDirection
	{
		X, Y, Z
	}

	[SerializeField]
	private GameEvent			m_OnOpen				= null;

	[SerializeField]
	private GameEvent			m_OnClose			   = null;

	[SerializeField]
	private	EAxisDirection		m_OperatingAxis		 = EAxisDirection.Z;
	
	[SerializeField]
	private	bool			   m_Opened					= false;

	[SerializeField][Range(0.001f, 2f)]
	private	float				m_LocalMovement			= 1f;

	[SerializeField][Range(0.001f, 2f)]
	private	float				m_TransitionTime		= 1f;

	private	bool				m_InTransition			= false;
	private	Rigidbody			m_Rigidbody				= null;


	//////////////////////////////////////////////////////////////////////////
	// START
	private void Start()
	{
		m_Rigidbody = GetComponentInChildren<Rigidbody>();

		if (m_Rigidbody == null )
			m_Rigidbody = gameObject.AddComponent<Rigidbody>();

		m_Rigidbody.useGravity = false;
		m_Rigidbody.isKinematic = true;
		m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		OnClose();
	}


	//////////////////////////////////////////////////////////////////////////
	// Activate
	public override void OnActivation()
	{
		if (m_InTransition )
			return;

		m_InTransition = true;

		CoroutinesManager.Start(Traslation(), "ControlledDrawer::OnActivation: Activation of " + name );
	}


	private Vector3 overlapBoxSize = new Vector3( 0.5f, 0.2f, 0.5f );
	//////////////////////////////////////////////////////////////////////////
	// OnOpen
	private void	OnOpen()
	{
		Collider[] colliders = Physics.OverlapBox(transform.position, overlapBoxSize, transform.rotation );
		foreach (Collider coll in colliders)
		{
			if (coll.TryGetComponent(out IInteractable interactable))
			{
				interactable.CanInteract = true;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnClose
	private void OnClose()
	{
		Collider[] colliders = Physics.OverlapBox(transform.position, overlapBoxSize, transform.rotation );
		foreach( Collider coll in colliders )
		{
			if (coll.TryGetComponent(out IInteractable interactable))
			{
				interactable.CanInteract = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Traslation
	private IEnumerator Traslation()
	{
		Vector3 startPosition	= transform.position;
		Vector3 endPosition		= Vector3.zero;

		// When is opening
		if (m_Opened == false )
		{

		}

		// When is closing
		if (m_Opened == true )
		{
			OnClose(); // Disable interactions
		}

		switch(m_OperatingAxis )
		{
			case EAxisDirection.X: endPosition = startPosition + ( transform.right.normalized   * (m_Opened ? -m_LocalMovement : m_LocalMovement)); break;
			case EAxisDirection.Y: endPosition = startPosition + ( transform.up.normalized	    * (m_Opened ? -m_LocalMovement : m_LocalMovement)); break;
			case EAxisDirection.Z: endPosition = startPosition + ( transform.forward.normalized * (m_Opened ? -m_LocalMovement : m_LocalMovement)); break;
		}
		
		float interpolant = 0f;
		float currentTime = 0f;

		while( interpolant < 1.0f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / m_TransitionTime;
			m_Rigidbody.MovePosition( Vector3.Lerp( startPosition, endPosition, interpolant ) );
			yield return null;
		}

		// When is opened
		if (m_Opened == false )
		{
			OnOpen();
			if (m_OnOpen != null && m_OnOpen.GetPersistentEventCount() > 0 )
			{
				m_OnOpen.Invoke();
			}
		}

		// When is closed
		if (m_Opened == true )
		{
			if (m_OnClose != null && m_OnClose.GetPersistentEventCount() > 0)
			{
				m_OnClose.Invoke();
			}
		}

		transform.position = endPosition;

		m_Opened = !m_Opened;
		m_InTransition = false;
	}

}
