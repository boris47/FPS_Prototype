
using UnityEngine;
using System.Collections;


public partial class Player : Human {

	[Header("Player Properties")]

	private	const	float			MAX_INTERACTION_DISTANCE		= 20f;

	public	static	Player			Instance						= null;
	public	static	IEntity			Entity							= null;

	// DASHING
	private		bool				m_IsDodging						= false;
	public		bool				IsDodging
	{
		get { return m_IsDodging; }
	}
	private		DashTarget			m_CurrentDashTarget				= null;
	private		DashTarget			m_PreviousDashTarget			= null;

	private		Grabbable			m_Grabbable						= null;
	private		IInteractable		m_Interactable					= null;
	private		Transform			m_DashAbilityTarget				= null;

	private		Vector3				m_Move							= Vector3.zero;

	private		ITourchLight		m_TourchLight					= null;

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
	// AWAKE
	protected override void Awake()
	{
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		Entity = this as IEntity;
		DontDestroyOnLoad( this );

		base.Awake();

		// Player Components
		{
			// TourchLight
			m_TourchLight		= transform.Find("TourchLight").GetComponent<ITourchLight>();
			// Foots
			m_Foots				= transform.Find( "FootSpace" ).GetComponent<IFoots>();

			m_DashAbilityTarget = transform.Find( "DashAbilityTarget" );
			m_DashAbilityTarget.localScale = new Vector3( 0.5f, m_PhysicCollider.height, 0.5f );
			m_DashAbilityTarget.gameObject.SetActive( false );
		}


		m_PlayerNearAreaTrigger	= transform.Find( "PNAT" ).GetComponent<Collider>(); // Player Near Area Trigger
		m_PlayerFarAreaTrigger	= transform.Find( "PFAT" ).GetComponent<Collider>(); // Player Far  Area Trigger

		// Player Data
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				Destroy( gameObject );
				return;
			}

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
		m_Stamina			= 1.0f;
		GroundSpeedModifier = 1.0f;
		m_IsActive			= true;

		SetMotionType( eMotionType.Walking );

		m_GrabPoint = new GameObject( "GrabPoint" );
		m_GrabPoint.transform.SetParent( transform );
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate( 0f, 0f, m_UseDistance );
		IsGrounded = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private void Start()
	{
		IsGrounded = true;
		StartCoroutine( DamageEffectCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// DropEntityDragged
	public	void	DropEntityDragged()
	{
		if ( m_GrabbedObject == null )
			return;

		Rigidbody rb		= m_GrabbedObject.GetComponent<Rigidbody>();
		rb.useGravity		= m_GrabbedObjectUseGravity;
		rb.mass				= m_GrabbedObjectMass;
		m_GrabbedObject		= null;
		m_CanGrabObjects	= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// MoveGrabbedObject
	private	void	MoveGrabbedObject()
	{
		if ( m_IsActive == false )
			return;

		if ( m_GrabbedObject == null )
			return;

		float distance = ( m_GrabbedObject.transform.position - m_GrabPoint.transform.position ).sqrMagnitude;
		if ( distance > m_UseDistance * m_UseDistance + 0.1f )
		{
			DropEntityDragged();
			return;
		}

		Rigidbody rb = m_GrabbedObject.GetComponent<Rigidbody>();
		rb.rotation = CameraControl.Instance.transform.rotation;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = ( m_GrabPoint.transform.position - m_GrabbedObject.transform.position ) / ( Time.fixedDeltaTime * 4f )
		* ( 1.0f - Vector3.Angle( transform.forward, CameraControl.Instance.transform.forward ) / CameraControl.CLAMP_MAX_X_AXIS );
	}


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void	FixedUpdate()
	{
		if ( m_IsActive == false )
			return;

		MoveGrabbedObject();

		m_RigidBody.angularVelocity = Vector3.zero;

		// Forced by ovverride
		if ( m_MovementOverrideEnabled )
		{
			if ( ( m_Move.x != 0.0f ) && ( m_Move.z != 0.0f  ) )
			{
				m_Move *= 0.707f;
			}

			m_Move *= GroundSpeedModifier;
			m_RigidBody.velocity = m_Move;
			return;
		}

		// User inputs
		if ( IsGrounded )
		{
			Vector3 forward = -Vector3.Cross( transform.up, CameraControl.Instance.transform.right );
			m_Move = ( m_MoveSmooth * forward ) + ( m_StrafeSmooth * CameraControl.Instance.transform.right );
			if ( ( m_Move.x != 0.0f ) && ( m_Move.z != 0.0f  ) )
			{
				m_Move *= 0.707f;
			}

			m_Move *= GroundSpeedModifier;
			m_RigidBody.velocity = m_Move;
		}
		else
		{
			// add gravity force
			m_RigidBody.AddForce( transform.up * Physics.gravity.y, ForceMode.Acceleration );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		this.OnFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForDash
	private	void	CheckForDash( bool hasHit )
	{
		if ( m_GrabbedObject != null )
			return;

//		if ( m_IsDashing == true )
//			return;

		// auto fall
		if ( IsGrounded == false && m_IsDodging == false && transform.up != Vector3.up )
		{
			RaycastHit hit;
			Physics.Raycast( transform.position, Vector3.down, out hit );

			if ( m_RotorDashCoroutine != null )
				StopCoroutine( m_RotorDashCoroutine );

//			m_IsDashing = true;

			Vector3 destination = hit.point + Vector3.up * m_DashAbilityTarget.localScale.y * 1.7f;
			m_RotorDashCoroutine = StartCoroutine( Dodge( destination: destination, destinationUp: Vector3.up, falling: true ) );
		}

		// if actually has no target
		if ( hasHit == false )
		{
			// if required reset last target
			if ( m_CurrentDashTarget != null )
			{
				m_CurrentDashTarget.HideText();
				m_CurrentDashTarget = null;
			}
			m_DashAbilityTarget.gameObject.SetActive( false );
			return;
		}

		DashTarget currentTarget = null;
		if ( hasHit == true )
		{
			currentTarget = m_RaycastHit.transform.GetComponent<DashTarget>();
			if ( currentTarget == null && m_CurrentDashTarget != null )
			{
				m_CurrentDashTarget.HideText();
				m_CurrentDashTarget = null;
				return;
			}
		}

		// hitting somewhere else
		if ( currentTarget == null )
		{
			if ( InputManager.Inputs.Ability1 )
			{
				float angle = Vector3.Angle( m_RaycastHit.normal, transform.up );
				bool validAngle = angle >= 89f && angle < 179f;
				m_DashAbilityTarget.gameObject.SetActive( validAngle );
				if ( validAngle )
				{
					m_DashAbilityTarget.position = m_RaycastHit.point;
					m_DashAbilityTarget.up = m_RaycastHit.normal;
				}
			}
			else
			{
				if ( m_DashAbilityTarget.gameObject.activeSelf )
				{
					if ( m_RotorDashCoroutine != null )
						StopCoroutine( m_RotorDashCoroutine );

					Vector3 destination = m_DashAbilityTarget.position + m_DashAbilityTarget.up;
					m_RotorDashCoroutine = StartCoroutine( Dodge( destination: destination, destinationUp: m_RaycastHit.normal ) );
				}
				m_DashAbilityTarget.gameObject.SetActive( false );
			}
		}


		// Can be a dash target
		if ( currentTarget != null && currentTarget != m_CurrentDashTarget )
		{
			// First target
			if ( currentTarget != null && m_CurrentDashTarget == null )
			{
				m_CurrentDashTarget = currentTarget;
				m_CurrentDashTarget.ShowText();
			}
			// New hit
			if ( currentTarget != null && m_CurrentDashTarget != null && currentTarget != m_CurrentDashTarget )
			{
				m_CurrentDashTarget.HideText();
				currentTarget.ShowText();
				m_CurrentDashTarget = currentTarget;
			}
			// No hit, reset previous
			if ( currentTarget == null && m_CurrentDashTarget != null )
			{
				m_CurrentDashTarget.HideText();
				m_CurrentDashTarget = null;
			}
		}

		// ACTION DASH
		if ( InputManager.Inputs.Use )
		{
			if ( m_CurrentDashTarget != null )
			{
				OnDashTargetUsed( ref m_CurrentDashTarget );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForInteraction
	private	void	CheckForInteraction( bool hasHit )
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

		// Distance check
		if ( m_RaycastHit.distance <= m_UseDistance )
		{
			m_Interactable = m_RaycastHit.transform.GetComponent<IInteractable>();
		}

		// ACTION INTERACT
		if ( InputManager.Inputs.Use )
		{
			if ( m_Interactable != null && m_Interactable.CanInteract )
			{
				m_Interactable.OnInteraction();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForGrab
	private	void	CheckForGrab( bool hasHit )
	{
		// skip if no target
		if ( hasHit == false )
		{
//			m_Grabbable = null;
			return;
		}

		// skip grab evaluation is dash is active
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

		// Distance check
		if ( m_CanGrabObjects == true && m_RaycastHit.distance <= m_UseDistance )
		{
			m_Grabbable = m_RaycastHit.transform.GetComponent<Grabbable>();
		}

		// ACTION GRAB
		if ( InputManager.Inputs.Use )
		{
			if ( m_GrabbedObject != null )
			{
				DropEntityDragged();
				return;
			}

			if ( m_Grabbable != null && m_Interactable != null && m_Interactable.CanInteract )
			{
				m_GrabbedObject = m_RaycastHit.transform.gameObject;

				Rigidbody rb = m_Grabbable.RigidBody;
				m_GrabbedObjectMass			= rb.mass;			rb.mass = 1f;
				m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity = false;
//				rb.interpolation = RigidbodyInterpolation.Extrapolate;
				m_CanGrabObjects = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame
	public override void	OnFrame( float deltaTime )
	{
		if ( m_IsActive == false )
			return;

		// Reset "local" states
		m_States.Reset();

		if ( InputManager.Inputs.ItemAction3 )
		{
			m_TourchLight.Toggle();
		}

		////////////////////////////////////////////////////////////////////////////////////////
		// Pick eventual collision info from camera to up
		{
			// my check hight formula
///			Leadwerks::Vec3 vCheckHeigth = { 0.0f, ( CamManager()->GetStdHeight() / 2 ) * ( fJumpForce / 10 ), 0.0f };
///			vCheckHeigth *= IsCrouched() ? 0.5f : 1.0f;
///			bIsUnderSomething = World()->GetWorld()->Pick( vCamPos, vCamPos + vCheckHeigth, Leadwerks::PickInfo(), 0.36 );
		}

		////////////////////////////////////////////////////////////////////////////////////////
		// Check for usage
#region		INTERACTIONS
		{
			Vector3 startLine = CameraControl.Instance.transform.position;
			Vector3 endLine = CameraControl.Instance.transform.position + CameraControl.Instance.transform.forward * MAX_INTERACTION_DISTANCE;

			bool lineCastResult = Physics.Linecast( startLine, endLine, out m_RaycastHit );

			Debug.DrawLine( startLine, endLine );

			CheckForDash ( lineCastResult );
			CheckForInteraction( lineCastResult );
			CheckForGrab ( lineCastResult );
			
			// Update Grab point position
			m_GrabPoint.transform.position = CameraControl.Instance.transform.position + ( CameraControl.Instance.transform.forward * m_UseDistance );
			m_GrabPoint.transform.rotation = CameraControl.Instance.transform.rotation;
		}

#endregion

		if ( m_IsDodging == true )
			return;

#region TO IMPLEMENT
		////////////////////////////////////////////////////////////////////////////////////////
		// Water
		/*		bool bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater;
				if ( !IsClimbing() && World()->GetWorld()->GetWaterMode() ) {

					float fWaterHeight		 = World()->GetWorld()->GetWaterHeight();
					// camera is under water level
					bIsCameraUnderWater = ( vCamPos.y - 0.1f ) < fWaterHeight;
					bIsCameraReallyUnderWater = vCamPos.y < fWaterHeight;
					// entity is under water level, but camera is over water level
					bIsEntityInWater = pEntity->GetPosition().y-0.1 < fWaterHeight && !bIsCameraUnderWater;

					SetInWater( bIsEntityInWater );

					// If now camera is over water level, but prev update was under it
					if ( bIsEntityInWater ) {

						// if distance beetwen ground and parent is minus than camera height
						if ( GetAirbourneHeigth() < CamManager()->GetStdHeight() ) {
							// restore walking state
						//	if ( iMotionType != LIVE_ENTITY::Motion::Walk ) {
								SetMotionType( LIVE_ENTITY::Motion::Walk );
							//	SetCrouched( true );
						//	}
						}

					}

					// If camera go under water level enable underwater state
					if ( bIsCameraUnderWater && iMotionType != LIVE_ENTITY::Motion::Swim ) {
						SetSwimming();
					}

					// if actual motion is 'Swim' but is not entity and camera underwater restore 'walk' motion
					if ( iMotionType == LIVE_ENTITY::Motion::Swim && !bIsEntityInWater && !bIsCameraUnderWater )
						SetMotionType( LIVE_ENTITY::Motion::Walk );

					if ( bIsCameraReallyUnderWater ) {
						SetUnderWater( true );

						// Underwater stamina is consumed as oxygen
						fStamina -= fRunStamina * 2.0f;
					}

				}
		*/
#endregion


		////////////////////////////////////////////////////////////////////////////////////////
		// Movement Update
		{
			switch ( MotionType )
			{
				case eMotionType.Walking:	{ this.Update_Walk();		break; }
				case eMotionType.Flying:	{ this.Update_Fly();		break; }
				case eMotionType.Swimming:	{ this.Update_Swim();		break; }
//				case eMotionType.Swimming:	{ this->Update_Swim( bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater );	break; }
				case eMotionType.P1ToP2:	{ this.Update_P1ToP2();		break; }
			}
		}

		if ( WeaponManager.Instance.CurrentWeapon.FirePoint1 != null )
		{
			m_TourchLight.Transform.position = WeaponManager.Instance.CurrentWeapon.FirePoint1.position;
			m_TourchLight.Transform.forward  = WeaponManager.Instance.CurrentWeapon.FirePoint1.forward;
		}

		// trace previuos states
		m_PreviousStates = m_States;
	}


	//////////////////////////////////////////////////////////////////////////
	// DamageEffectCO ( Coroutine )
	private	IEnumerator	DamageEffectCO()
	{
		while( true )
		{
			var settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
			m_DamageEffect = Mathf.Lerp( m_DamageEffect, 0f, Time.deltaTime * 2f );
			settings.intensity = m_DamageEffect;
			CameraControl.Instance.GetPP_Profile.vignette.settings = settings;
			yield return null;
		}
	}


	public override void OnThink()
	{ }

}
