
using UnityEngine;
using System.Collections;
using System;

public partial class Player : Human {

	[Header("Player Properties")]

	public LayerMask mask;

	private	const	float			MAX_INTERACTION_DISTANCE		= 2.1f;

	private	const	float			BODY_DRAG						= 8f;

	public	static	Player			Instance						= null;
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
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		Entity = this as IEntity;
		DontDestroyOnLoad( this );


		m_EntityType = ENTITY_TYPE.ACTOR;

		m_SectionName = this.GetType().FullName;

		base.Awake();

		m_PlayerNearAreaTrigger	= transform.Find( "PNAT" ).GetComponent<Collider>(); // Player Near Area Trigger
		m_PlayerFarAreaTrigger	= transform.Find( "PFAT" ).GetComponent<Collider>(); // Player Far  Area Trigger

		// Player Components
		{
			// Foots
			Transform foots		= transform.Find( "FootSpace" );
			m_Foots				= foots.GetComponent<IFoots>();
			Collider footsCollider = foots.GetComponent<Collider>();
			this.DisableCollisionsWith( footsCollider );

			m_DodgeAbilityTarget = transform.Find( "DodgeAbilityTarget" );
			m_DodgeAbilityTarget.SetParent( null );
			m_DodgeAbilityTarget.gameObject.SetActive( false );
		}

		// Player Data
		{
			// Walking
			m_SectionRef.AsMultiValue( "Walk",		1, 2, 3, ref m_WalkSpeed,	ref m_WalkJumpCoef,		ref m_WalkStamina );
			
			// Running
			m_SectionRef.AsMultiValue( "Run",		1, 2, 3, ref m_RunSpeed,	ref m_RunJumpCoef,		ref m_RunStamina );

			// Crouched
			m_SectionRef.AsMultiValue( "Crouch",	1, 2, 3, ref m_CrouchSpeed,	ref m_CrouchJumpCoef,	ref m_CrouchStamina );

			m_FallDistanceThreshold = m_SectionRef.AsFloat( "FallDistanceThreshold" );

			// Climbing
///			bool result = m_SectionRef.bAsFloat( "Climb", ref m_ClimbSpeed );
			m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", 0.12f );
//			m_ClimbSpeed				= m_SectionRef[ "Climb" ].Value.ToFloat();

			// Jumping
			{
//				cMultiValue JumpInfos	= m_SectionRef[ "Jump" ].MultiValue;
//				m_JumpForce				= JumpInfos[ 0 ].As<float>();
//				m_JumpForce				= JumpInfos[ 0 ].ToFloat();
//				m_JumpForce				= JumpInfos[ 0 ];

				m_SectionRef.AsMultiValue( "Jump", 1, 2,	ref m_JumpForce,	ref m_JumpStamina );
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

		SetMotionType( eMotionType.Walking );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Start()
	{
		if ( CameraControl.Instance.Transform.IsChildOf( m_HeadTransform ) == false )
		{
			Debug.Log( "Camera not parented with player head. Setting player's head as parent of camera" );
			CameraControl.Instance.SetViewPoint( m_HeadTransform );
		}

		IsGrounded = false;
		StartCoroutine( DamageEffectCO() );


		m_GrabPoint = new GameObject( "GrabPoint" );
		m_GrabPoint.transform.SetParent( CameraControl.Instance.Transform /* CameraControl.Instance.Transform */);
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate( 0f, 0f, m_UseDistance );
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

		if ( m_IsDodging == true )
			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public					void	DropEntityDragged()
	{
		if ( m_GrabbedObject == null )
			return;

		m_GrabPoint.transform.localPosition = Vector3.forward * m_UseDistance;

		Rigidbody rb		= m_GrabbedObject.Interactable.RigidBody;
		rb.useGravity		= m_GrabbedObjectUseGravity;
		rb.mass				= m_GrabbedObjectMass;

		Physics.IgnoreCollision( m_PhysicCollider, m_GrabbedObject.Interactable.Collider, ignore: false );

		m_GrabbedObject		= null;
		m_CanGrabObjects	= true;
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	MoveGrabbedObject()
	{
		if ( m_IsActive == false )
			return;

		if ( m_GrabbedObject == null )
			return;

		float distance = ( m_GrabbedObject.Transform.position - m_GrabPoint.transform.position ).sqrMagnitude;
		if ( distance > m_UseDistance * m_UseDistance + 0.1f )
		{
			DropEntityDragged();
			return;
		}

		Rigidbody rb = m_GrabbedObject.Interactable.RigidBody;
		rb.rotation = CameraControl.Instance.Transform.rotation;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = ( m_GrabPoint.transform.position - m_GrabbedObject.Transform.position ) / ( Time.fixedDeltaTime * 4f );
//		* ( 1.0f - Vector3.Angle( transform.forward, CameraControl.Instance.transform.forward ) / CameraControl.CLAMP_MAX_X_AXIS );
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForInteraction( bool hasHit )
	{
		// skip if no target
		if ( hasHit == false )
		{
			m_Interactable = null;
			return;
		}

		// skip if currently grabbing an object
		if ( m_GrabbedObject != null )
			return;

		// skip if currently dask is active
		if ( m_IsDodging == true )
			return;

		bool m_bHasComponent = false;
		// Distance check
		if ( m_RaycastHit.distance <= m_UseDistance )
		{
			m_bHasComponent = Utils.Base.SearchComponent( m_RaycastHit.transform.gameObject,ref m_Interactable, SearchContext.LOCAL );
		}

		// ACTION INTERACT
		if ( InputManager.Inputs.Use )
		{
			if ( m_bHasComponent && m_Interactable.CanInteract )
			{
				m_Interactable.OnInteraction();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private					void	CheckForGrab( bool hasHit )
	{
		// skip grab evaluation if dodging
		if ( m_IsDodging == true )
			return;

		// ACTION RELEASE
		if ( InputManager.Inputs.Use )
		{
			if ( m_GrabbedObject != null )
			{
				DropEntityDragged();
				return;
			}
		}

		// Skip if cannot grab objects
		if ( m_CanGrabObjects == false )
			return;

		// skip if no target
		if ( hasHit == false )
			return;

		// GRAB ACTION
		Grabbable grabbable = null;
		bool m_bHasComponent = Utils.Base.SearchComponent( m_RaycastHit.transform.gameObject,ref grabbable, SearchContext.LOCAL );
		if ( InputManager.Inputs.Use && m_bHasComponent && grabbable.CanInteract )
		{
			m_GrabbedObject = grabbable;
			m_GrabPoint.transform.localPosition = Vector3.forward * Vector3.Distance( transform.position, grabbable.transform.position );

			Rigidbody rb				= grabbable.RigidBody;
			m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
			m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
			rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
			m_CanGrabObjects			= false;

			Physics.IgnoreCollision( m_PhysicCollider, grabbable.Collider, ignore: true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private					IEnumerator	DamageEffectCO()
	{
		var settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		while( true )
		{
			m_DamageEffect = Mathf.Lerp( m_DamageEffect, 0f, Time.deltaTime * 2f );
			settings.intensity = m_DamageEffect;
			CameraControl.Instance.GetPP_Profile.vignette.settings = settings;
			yield return null;
		}
	}
}
