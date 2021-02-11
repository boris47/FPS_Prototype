﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Player : Human {

	[Header("Player Properties")]

	private	const	float			BODY_DRAG						= 8f;

	public	static	Player			m_Instance						= null;
	public	static	Player			Instance
	{
		get { return m_Instance; }
	}
	public	static	IEntity			Entity							= null;

	private		IInteractable		m_Interactable					= null;

	// GRABBING OBJECT
	[System.NonSerialized]
	protected	GameObject			m_GrabPoint						= null;

	protected	IGrabbable			m_GrabbedObject					= null;
	public		IGrabbable			GrabbedObject
	{
		get { return m_GrabbedObject; }
	}

	[System.NonSerialized]
	protected	float				m_GrabbedObjectMass				= 0f;

	[System.NonSerialized]
	protected	bool				m_GrabbedObjectUseGravity		= false;
	protected	bool				m_CanGrabObjects				= true;

	private		Vector3				m_Move							= Vector3.zero;

	private		RaycastHit			m_RaycastHit;
	[SerializeField]
	private		bool				m_HasRaycasthit					= false;

	private		Collider			m_PlayerNearAreaTrigger			= null;
	public		Collider			PlayerNearAreaTrigger
	{
		get { return m_PlayerNearAreaTrigger; }
	}

	private		Collider			m_PlayerFarAreaTrigger			= null;
	public		Collider			PlayerFarAreaTrigger
	{
		get { return m_PlayerFarAreaTrigger; }
	}



	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		m_EntityType = EEntityType.ACTOR;

		if ( m_Instance != null )
		{
			Destroy(gameObject );
			return;
		}

		m_Instance = this;
		Entity = this as IEntity;
		DontDestroyOnLoad( this );

		m_SectionName = GetType().FullName;

		base.Awake();

		transform.TrySearchComponentByChildName( "PNAT", out m_PlayerNearAreaTrigger ); // Player Near Area Trigger
		transform.TrySearchComponentByChildName( "PFAT", out m_PlayerFarAreaTrigger  ); // Player Far  Area Trigger

		// Player Components
		{
			// Foots
			Utils.Base.TrySearchComponent(gameObject, ESearchContext.CHILDREN, out m_Foots );
			DisableCollisionsWith(m_Foots.Collider );

//			this.m_DodgeAbilityTarget = this.transform.Find( "DodgeAbilityTarget" );
//			this.m_DodgeAbilityTarget.SetParent( null );
//			this.m_DodgeAbilityTarget.gameObject.SetActive( false );
		}

		// Player Data
		{
			// Walking
			m_SectionRef.AsMultiValue( "Walk",		1, 2, 3, out m_WalkSpeed,	out m_WalkJumpCoef,		out m_WalkStamina );

			// Running
			m_SectionRef.AsMultiValue( "Run",		1, 2, 3, out m_RunSpeed,	out m_RunJumpCoef,		out m_RunStamina );

			// Crouched
			m_SectionRef.AsMultiValue( "Crouch",	1, 2, 3, out m_CrouchSpeed, out m_CrouchJumpCoef,	out m_CrouchStamina );

			m_FallDistanceThreshold		= m_SectionRef.AsFloat( "FallDistanceThreshold" );

			// Climbing
			///			bool result = m_SectionRef.bAsFloat( "Climb", ref m_ClimbSpeed );
			m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", 0.12f );
//			m_ClimbSpeed				= m_SectionRef[ "Climb" ].Value.ToFloat();

			// Jumping
			{
				m_SectionRef.AsMultiValue( "Jump", 1, 2, out m_JumpForce, out m_JumpStamina );
			}

			// Stamina
			{
				m_StaminaRestore		= m_SectionRef.AsFloat( "StaminaRestore", 0.0f );
				m_StaminaRunMin			= m_SectionRef.AsFloat( "StaminaRunMin",  0.3f );
				m_StaminaJumpMin		= m_SectionRef.AsFloat( "StaminaJumpMin", 0.4f );
			}

		}

		m_Health			= m_SectionRef.AsFloat( "Health", 100.0f );
		m_RigidBody.mass	= m_SectionRef.AsFloat( "phMass", 80.0f  );
		m_RigidBody.maxAngularVelocity = 0f;
		m_RigidBody.useGravity = false;
		m_Stamina			= 1.0f;
		GroundSpeedModifier = 1.0f;
		IsGrounded			= false;
		m_IsActive			= true;


		m_OnMotionStateChangedEvent += OnMotionTypeChanged;
		SetMotionType( EMotionType.Walking );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnMotionTypeChanged( EMotionType prevState, EMotionType newState )
	{
		switch ( prevState )
		{
			case EMotionType.Walking:
				UnRegisterGroundedMotion();
				break;
			case EMotionType.Flying:
				UnRegisterFlyMotion();
				break;
			case EMotionType.Swimming:
				UnRegisterSwimMotion();
				break;
			case EMotionType.P1ToP2:
				UnRegisterP1ToP2Motion();
				break;
			default:
				break;
		}

		switch ( newState )
		{
			case EMotionType.Walking:
				RegisterGroundedMotion();
				break;
			case EMotionType.Flying:
				RegisterFlyMotion();
				break;
			case EMotionType.Swimming:
				RegisterSwimMotion();
				break;
			case EMotionType.P1ToP2:
				RegisterP1ToP2Motion();
				break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Start()
	{
		/*
		if ( CameraControl.Instance.Transform.IsChildOf( m_HeadTransform ) == false )
		{
			Debug.Log( "Player::Start: Camera not parented with player head. Setting player's head as parent of camera" );
			CameraControl.Instance.SetViewPoint( m_HeadTransform );
		}
		*/

		CameraControl.Instance.SetViewPoint(m_HeadTransform, m_BodyTransform );

		IsGrounded = false;

		m_GrabPoint = new GameObject( "GrabPoint" );
		m_GrabPoint.transform.SetParent( CameraControl.Instance.transform );
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate( 0f, 0f, m_UseDistance );
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		base.OnDisable();

		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_FORWARD,	"ForwardEvent" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_BACKWARD,	"BackwardEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_LEFT,		"LeftEvent" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_RIGHT,	"RightEvent" );
 
		GlobalManager.InputMgr.UnbindCall( EInputCommands.STATE_RUN,		"RunEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.STATE_JUMP,	"JumpEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.USAGE,			"Interaction" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.USAGE,			"Grab" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.GADGET3,		"Flashlight" );

//		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_PRESS,	"DodgeStart" );
//		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_HOLD,	"DodgeContinue" );
//		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_RELEASE,"DodgeEnd" );
	}


	//////////////////////////////////////////////////////////////////////////
	public					void	DisableCollisionsWith( Collider collider, bool bAlsoTriggerCollider = true )
	{
//		if ( bAlsoTriggerCollider )
	//	{
	//		Physics.IgnoreCollision( collider, m_TriggerCollider, ignore: true );
	//	}
		Physics.IgnoreCollision( collider, m_PhysicCollider, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerNearAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerFarAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, m_Foots.Collider, ignore: true );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	CanTrigger()
	{
		if ( base.CanTrigger() == false )
			return false;

//		if (this.m_IsDodging == true )
//			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void	DropEntityDragged()
	{
		if (m_GrabbedObject == null )
			return;

		m_GrabPoint.transform.localPosition = Vector3.forward * m_UseDistance;

		Rigidbody rb		= m_GrabbedObject.Interactable.RigidBody;
		rb.useGravity		= m_GrabbedObjectUseGravity;
		rb.mass				= m_GrabbedObjectMass;

		if ( m_GrabbedObject.Transform.TryGetComponent(out OnHitEventGrabbedHandler eventHandler) )
		{
			Destroy( eventHandler );
		}

		m_GrabbedObject.Interactable.OnRetroInteraction();

		Physics.IgnoreCollision(m_PhysicCollider, m_GrabbedObject.Interactable.Collider, ignore: false );

		m_GrabbedObject		= null;
		m_CanGrabObjects	= true;
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	MoveGrabbedObject()
	{
		if (m_IsActive == false )
			return;

		if (m_GrabbedObject == null )
			return;

		float distance = (m_GrabbedObject.Transform.position - m_GrabPoint.transform.position ).sqrMagnitude;
		if ( distance > (m_UseDistance * m_UseDistance) + 0.1f )
		{
			DropEntityDragged();
			return;
		}

		Rigidbody rb = m_GrabbedObject.Interactable.RigidBody;
		rb.rotation = CameraControl.Instance.transform.rotation;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = (m_GrabPoint.transform.position - m_GrabbedObject.Transform.position ) / ( Time.fixedDeltaTime * 4f );
//		* ( 1.0f - Vector3.Angle( transform.forward, CameraControl.Instance.transform.forward ) / CameraControl.CLAMP_MAX_X_AXIS );
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForInteraction( bool hasHit )
	{
		//// skip if no target
		//if ( hasHit == false )
		//{
		//	m_Interactable = null;
		//	return;
		//}

		//// skip if currently grabbing an object
		//if ( m_GrabbedObject != null )
		//	return;

		//// skip if currently dask is active
		//if ( m_IsDodging == true )
		//	return;

		//bool m_HasComponent = false;
		//// Distance check
		//if ( m_RaycastHit.distance <= m_UseDistance )
		//{
		//	m_HasComponent = Utils.Base.TrySearchComponent( m_RaycastHit.transform.gameObject,ref m_Interactable, SearchContext.LOCAL );
		//}

		//// ACTION INTERACT
		//if ( InputManager.Inputs.Use )
		//{
		//	if ( m_HasComponent && m_Interactable.CanInteract )
		//	{
		//		m_Interactable.OnInteraction();
		//	}
		//}

	}


	private	bool	InteractionPredicate()
	{
		return (/*this.m_IsDodging == false && */m_GrabbedObject == null && m_HasRaycasthit && m_RaycastHit.distance <= m_UseDistance && m_Interactable != null && m_Interactable.CanInteract );
	}


	private	void	InteractionAction()
	{
		m_Interactable.OnInteraction();
/*
		if ( m_Interactable.HasRetroInteraction && m_Interactable.HasInteracted )
		{
			m_Interactable.OnRetroInteraction();
		}
		else
		{
			m_Interactable.OnInteraction();
		}
*/
	}



	private	bool	GrabPredicate()
	{
		return m_HasRaycasthit && m_RaycastHit.distance <= m_UseDistance;// m_CanGrabObjects == true;
	}

	private	void	GrabAction()
	{
		if (m_GrabbedObject != null )
		{
			DropEntityDragged();
			return;
		}


		// GRAB ACTION
		bool m_HasComponent = Utils.Base.TrySearchComponent( m_RaycastHit.transform.gameObject, ESearchContext.LOCAL, out Grabbable grabbable );
		if ( m_HasComponent && grabbable.CanInteract )
		{
			grabbable.OnInteraction();
			m_GrabbedObject = grabbable;
			m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance(transform.position, grabbable.transform.position );

			Rigidbody rb				= grabbable.RigidBody;
			m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
			m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
			rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
			m_CanGrabObjects			= false;

			Physics.IgnoreCollision(m_PhysicCollider, grabbable.Collider, ignore: true );

			grabbable.gameObject.AddComponent<OnHitEventGrabbedHandler>();
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForGrab( bool hasHit )
	{
		//// skip grab evaluation if dodging
		//if ( m_IsDodging == true )
		//	return;

		//// ACTION RELEASE
		//if ( InputManager.Inputs.Use )
		//{
		//	if ( m_GrabbedObject != null )
		//	{
		//		DropEntityDragged();
		//		return;
		//	}
		//}

		//// Skip if cannot grab objects
		//if ( m_CanGrabObjects == false )
		//	return;

		//// skip if no target
		//if ( hasHit == false )
		//	return;

		//	// GRAB ACTION
		//	Grabbable grabbable = null;
		//	bool m_HasComponent = Utils.Base.TrySearchComponent( m_RaycastHit.transform.gameObject, ref grabbable, SearchContext.LOCAL );
		//	if ( InputManager.Inputs.Use && m_HasComponent && grabbable.CanInteract )
		//	{
		//		m_GrabbedObject = grabbable;
		//		m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance( transform.position, grabbable.transform.position );

		//		Rigidbody rb				= grabbable.RigidBody;
		//		m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
		//		m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
		//		rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
		//		m_CanGrabObjects			= false;

		//		Physics.IgnoreCollision( m_PhysicCollider, grabbable.Collider, ignore: true );
		//	}
	}

}
