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
		this.m_Rigidbody = this.GetComponentInChildren<Rigidbody>();

		if (this.m_Rigidbody == null )
			this.m_Rigidbody = this.gameObject.AddComponent<Rigidbody>();

		this.m_Rigidbody.useGravity = false;
		this.m_Rigidbody.isKinematic = true;
		this.m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		this.OnClose();
	}


	//////////////////////////////////////////////////////////////////////////
	// Activate
	public override void OnActivation()
	{
		if (this.m_InTransition )
			return;

		this.m_InTransition = true;

		CoroutinesManager.Start(this.Traslation(), "ControlledDrawer::OnActivation: Activation of " + this.name );
	}


	private Vector3 overlapBoxSize = new Vector3( 0.5f, 0.2f, 0.5f );
	//////////////////////////////////////////////////////////////////////////
	// OnOpen
	private void	OnOpen()
	{
		Collider[] colliders = Physics.OverlapBox(this.transform.position, this.overlapBoxSize, this.transform.rotation );
		foreach (Collider coll in colliders)
		{
			IInteractable interactable = coll.GetComponent<IInteractable>();
			if ( interactable != null )
			{
				interactable.CanInteract = true;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnClose
	private void OnClose()
	{
		Collider[] colliders = Physics.OverlapBox(this.transform.position, this.overlapBoxSize, this.transform.rotation );
		foreach( Collider coll in colliders )
		{
		   IInteractable interactable = coll.GetComponent<IInteractable>();
			if ( interactable != null )
			{
				interactable.CanInteract = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Traslation
	private IEnumerator Traslation()
	{
		Vector3 startPosition	= this.transform.position;
		Vector3 endPosition		= Vector3.zero;

		// When is opening
		if (this.m_Opened == false )
		{

		}

		// When is closing
		if (this.m_Opened == true )
		{
			this.OnClose(); // Disable interactions
		}

		switch(this.m_OperatingAxis )
		{
			case EAxisDirection.X: endPosition = startPosition + ( this.transform.right.normalized   * (this.m_Opened ? -this.m_LocalMovement : this.m_LocalMovement)); break;
			case EAxisDirection.Y: endPosition = startPosition + ( this.transform.up.normalized	    * (this.m_Opened ? -this.m_LocalMovement : this.m_LocalMovement)); break;
			case EAxisDirection.Z: endPosition = startPosition + ( this.transform.forward.normalized * (this.m_Opened ? -this.m_LocalMovement : this.m_LocalMovement)); break;
		}
		
		float interpolant = 0f;
		float currentTime = 0f;

		while( interpolant < 1.0f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / this.m_TransitionTime;
			this.m_Rigidbody.MovePosition( Vector3.Lerp( startPosition, endPosition, interpolant ) );
			yield return null;
		}

		// When is opened
		if (this.m_Opened == false )
		{
			this.OnOpen();
			if (this.m_OnOpen != null && this.m_OnOpen.GetPersistentEventCount() > 0 )
			{
				this.m_OnOpen.Invoke();
			}
		}

		// When is closed
		if (this.m_Opened == true )
		{
			if (this.m_OnClose != null && this.m_OnClose.GetPersistentEventCount() > 0)
			{
				this.m_OnClose.Invoke();
			}
		}

		this.transform.position = endPosition;

		this.m_Opened = !this.m_Opened;
		this.m_InTransition = false;
	}

}
