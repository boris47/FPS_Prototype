
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Player : Human {

	[Header("Player Properties")]

	private	const	float			MAX_INTERACTION_DISTANCE		= 40.1f; // TODO set to 2.1

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
		get { return this.m_GrabbedObject; }
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
		get { return this.m_PlayerNearAreaTrigger; }
	}

	private		Collider			m_PlayerFarAreaTrigger			= null;
	public		Collider			PlayerFarAreaTrigger
	{
		get { return this.m_PlayerFarAreaTrigger; }
	}



	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		this.m_EntityType = EEntityType.ACTOR;

		if ( m_Instance != null )
		{
			Destroy(this.gameObject );
			return;
		}

		m_Instance = this;
		Entity = this as IEntity;
		DontDestroyOnLoad( this );

		this.m_SectionName = this.GetType().FullName;

		base.Awake();

		this.transform.SearchComponentInChild( "PNAT", ref this.m_PlayerNearAreaTrigger ); // Player Near Area Trigger
		this.transform.SearchComponentInChild( "PFAT", ref this.m_PlayerFarAreaTrigger  ); // Player Far  Area Trigger

		// Player Components
		{
			// Foots
			Utils.Base.SearchComponent(this.gameObject, out this.m_Foots, ESearchContext.CHILDREN );
			this.DisableCollisionsWith(this.m_Foots.Collider );

//			this.m_DodgeAbilityTarget = this.transform.Find( "DodgeAbilityTarget" );
//			this.m_DodgeAbilityTarget.SetParent( null );
//			this.m_DodgeAbilityTarget.gameObject.SetActive( false );
		}

		// Player Data
		{
			// Walking
			this.m_SectionRef.AsMultiValue( "Walk",		1, 2, 3, ref this.m_WalkSpeed,	ref this.m_WalkJumpCoef,		ref this.m_WalkStamina );

			// Running
			this.m_SectionRef.AsMultiValue( "Run",		1, 2, 3, ref this.m_RunSpeed,	ref this.m_RunJumpCoef,		ref this.m_RunStamina );

			// Crouched
			this.m_SectionRef.AsMultiValue( "Crouch",	1, 2, 3, ref this.m_CrouchSpeed,	ref this.m_CrouchJumpCoef,	ref this.m_CrouchStamina );

			this.m_FallDistanceThreshold		= this.m_SectionRef.AsFloat( "FallDistanceThreshold" );

			// Climbing
			///			bool result = m_SectionRef.bAsFloat( "Climb", ref m_ClimbSpeed );
			this.m_ClimbSpeed				= this.m_SectionRef.AsFloat( "Climb", 0.12f );
//			m_ClimbSpeed				= m_SectionRef[ "Climb" ].Value.ToFloat();

			// Jumping
			{
				//				Database.cMultiValue JumpInfos	= m_SectionRef[ "Jump" ].MultiValue;
				//	/*float*/	m_JumpForce				= JumpInfos[ 0 ].As<float>();				// Using System.Convert.ChangeType
				//	/*float*/	m_JumpForce				= JumpInfos[ 0 ].ToFloat();					// Same as before
				//	/*float*/	m_JumpForce				= JumpInfos[ 0 ];							// Implicit conversion

				this.m_SectionRef.AsMultiValue( "Jump", 1, 2,	ref this.m_JumpForce,	ref this.m_JumpStamina );

//				print( m_SectionRef[ "Jump" ].MultiValue[0].ToInteger() );
			}

			// Stamina
			{
				this.m_StaminaRestore		= this.m_SectionRef.AsFloat( "StaminaRestore", 0.0f );
				this.m_StaminaRunMin			= this.m_SectionRef.AsFloat( "StaminaRunMin",  0.3f );
				this.m_StaminaJumpMin		= this.m_SectionRef.AsFloat( "StaminaJumpMin", 0.4f );
			}

		}

		this.m_Health			= this.m_SectionRef.AsFloat( "Health", 100.0f );
		this.m_RigidBody.mass	= this.m_SectionRef.AsFloat( "phMass", 80.0f  );
		this.m_RigidBody.maxAngularVelocity = 0f;
		this.m_RigidBody.useGravity = false;
		this.m_Stamina			= 1.0f;
		this.GroundSpeedModifier = 1.0f;
		this.IsGrounded			= false;
		this.m_IsActive			= true;


		m_OnMotionStateChangedEvent += this.OnMotionTypeChanged;
		this.SetMotionType( EMotionType.Walking );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnMotionTypeChanged( EMotionType prevState, EMotionType newState )
	{
		switch ( prevState )
		{
			case EMotionType.Walking:
				this.UnRegisterGroundedMotion();
				break;
			case EMotionType.Flying:
				this.UnRegisterFlyMotion();
				break;
			case EMotionType.Swimming:
				this.UnRegisterSwimMotion();
				break;
			case EMotionType.P1ToP2:
				this.UnRegisterP1ToP2Motion();
				break;
			default:
				break;
		}

		switch ( newState )
		{
			case EMotionType.Walking:
				this.RegisterGroundedMotion();
				break;
			case EMotionType.Flying:
				this.RegisterFlyMotion();
				break;
			case EMotionType.Swimming:
				this.RegisterSwimMotion();
				break;
			case EMotionType.P1ToP2:
				this.RegisterP1ToP2Motion();
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

		CameraControl.Instance.SetViewPoint(this.m_HeadTransform, this.m_BodyTransform );

		this.IsGrounded = false;

		this.m_GrabPoint = new GameObject( "GrabPoint" );
		this.m_GrabPoint.transform.SetParent( CameraControl.Instance.Transform /* CameraControl.Instance.Transform */);
		this.m_GrabPoint.transform.localPosition = Vector3.zero;
		this.m_GrabPoint.transform.localRotation = Quaternion.identity;
		this.m_GrabPoint.transform.Translate( 0f, 0f, this.m_UseDistance );
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
		Physics.IgnoreCollision( collider, this.m_PhysicCollider, ignore: true );
		Physics.IgnoreCollision( collider, this.m_PlayerNearAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, this.m_PlayerFarAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, this.m_Foots.Collider, ignore: true );
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
		if (this.m_GrabbedObject == null )
			return;

		this.m_GrabPoint.transform.localPosition = Vector3.forward * this.m_UseDistance;

		Rigidbody rb		= this.m_GrabbedObject.Interactable.RigidBody;
		rb.useGravity		= this.m_GrabbedObjectUseGravity;
		rb.mass				= this.m_GrabbedObjectMass;

		if ( this.m_GrabbedObject.Transform.TryGetComponent(out OnHitEventGrabbedHandler eventHandler) )
		{
			Destroy( eventHandler );
		}

		this.m_GrabbedObject.Interactable.OnRetroInteraction();

		Physics.IgnoreCollision(this.m_PhysicCollider, this.m_GrabbedObject.Interactable.Collider, ignore: false );

		this.m_GrabbedObject		= null;
		this.m_CanGrabObjects	= true;
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	MoveGrabbedObject()
	{
		if (this.m_IsActive == false )
			return;

		if (this.m_GrabbedObject == null )
			return;

		float distance = (this.m_GrabbedObject.Transform.position - this.m_GrabPoint.transform.position ).sqrMagnitude;
		if ( distance > (this.m_UseDistance * this.m_UseDistance) + 0.1f )
		{
			this.DropEntityDragged();
			return;
		}

		Rigidbody rb = this.m_GrabbedObject.Interactable.RigidBody;
		rb.rotation = CameraControl.Instance.Transform.rotation;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = (this.m_GrabPoint.transform.position - this.m_GrabbedObject.Transform.position ) / ( Time.fixedDeltaTime * 4f );
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
		//	m_HasComponent = Utils.Base.SearchComponent( m_RaycastHit.transform.gameObject,ref m_Interactable, SearchContext.LOCAL );
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
		return (/*this.m_IsDodging == false && */this.m_GrabbedObject == null && this.m_HasRaycasthit && this.m_RaycastHit.distance <= this.m_UseDistance && this.m_Interactable != null && this.m_Interactable.CanInteract );
	}


	private	void	InteractionAction()
	{
		this.m_Interactable.OnInteraction();
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
		return this.m_HasRaycasthit && this.m_RaycastHit.distance <= this.m_UseDistance;// m_CanGrabObjects == true;
	}

	private	void	GrabAction()
	{
		if (this.m_GrabbedObject != null )
		{
			this.DropEntityDragged();
			return;
		}


		// GRAB ACTION
		bool m_HasComponent = Utils.Base.SearchComponent( this.m_RaycastHit.transform.gameObject, out Grabbable grabbable, ESearchContext.LOCAL );
		if ( m_HasComponent && grabbable.CanInteract )
		{
			grabbable.OnInteraction();
			this.m_GrabbedObject = grabbable;
			this.m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance(this.transform.position, grabbable.transform.position );

			Rigidbody rb				= grabbable.RigidBody;
			this.m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
			this.m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
			rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
			this.m_CanGrabObjects			= false;

			Physics.IgnoreCollision(this.m_PhysicCollider, grabbable.Collider, ignore: true );

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
		//	bool m_HasComponent = Utils.Base.SearchComponent( m_RaycastHit.transform.gameObject, ref grabbable, SearchContext.LOCAL );
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
