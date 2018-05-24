
using UnityEngine;
using System.Collections;

public partial class Player : Human {

	[Header("Player Properties")]

	private	const	float			MAX_INTERACTION_DISTANCE		= 20f;

	public	static	Player			Instance						= null;
	public	static	IEntity			Entity							= null;

	private		IInteractable		m_Interactable					= null;

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
			// Foots
			m_Foots				= transform.Find( "FootSpace" ).GetComponent<IFoots>();

			m_DodgeAbilityTarget = transform.Find( "DodgeAbilityTarget" );
			m_DodgeAbilityTarget.localScale = new Vector3( 0.5f, m_PhysicCollider.height, 0.5f );
			m_DodgeAbilityTarget.SetParent( null );

			m_DodgeAbilityTarget.gameObject.SetActive( false );
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
		m_GrabPoint.transform.SetParent( CameraControl.Instance.Transform );
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
	// OnSave ( override )
	protected override StreamingUnit OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit	= base.OnSave( streamingData );
		if ( streamingUnit == null )
			return null;

		// Health
		streamingUnit.AddInternal( "Health", m_Health );

		// Stamina
		streamingUnit.AddInternal( "Stamina", m_Stamina );

		// Crouch state
		streamingUnit.AddInternal( "IsCrouched", IsCrouched );

		// Motion Type
		streamingUnit.AddInternal( "MotionType", MotionType );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad ( override )
	protected override StreamingUnit OnLoad( StreamingData streamingData )
	{
		StreamingUnit streamingUnit = base.OnLoad( streamingData );
		if ( streamingUnit == null )
			return null;

		// Cutscene Manager
		if ( m_CutsceneManager.IsPlaying == true )
			m_CutsceneManager.Termiante();

		// UI effect reset
		UI.Instance.InGame.GetEffectFrame().color = Color.clear;

		// Dodging reset
		if ( m_DodgeCoroutine != null )
		{
			StopCoroutine( m_DodgeCoroutine );
		}
		m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity						= Vector3.zero;
		SoundEffectManager.Instance.Pitch			= 1f;
		Time.timeScale								= 1f;
		m_DodgeRaycastNormal						= Vector3.zero;
		m_DodgeAbilityTarget.gameObject.SetActive( false );
		m_ChosingDodgeRotation						= false;
		m_DodgeInterpolant							= 0f;
		var settings								= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		settings.frameBlending						= 0f;
		CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;
		m_IsDodging = false;

		// Player internals
		m_Interactable								= null;
		m_Move										= Vector3.zero;
		m_RaycastHit								= default( RaycastHit );

		DropEntityDragged();

		m_MoveSmooth = m_StrafeSmooth = 0f;

		// Health
		m_Health			= streamingUnit.GetAsFloat( "Health" );

		// Stamina
		m_Stamina			= streamingUnit.GetAsFloat( "Stamina" );

		// Crouch state
		m_States.IsCrouched = streamingUnit.GetAsBool( "IsCrouched" );

		// Motion Type
		MotionType			= streamingUnit.GetAsEnum<eMotionType>( "MotionType");
		SetMotionType( MotionType );

		return streamingUnit;
	}


	public	void	DisableCollisionsWith( Collider collider )
	{
		Physics.IgnoreCollision( collider, m_PhysicCollider, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerNearAreaTrigger, ignore: true );
		Physics.IgnoreCollision( collider, m_PlayerFarAreaTrigger, ignore: true );
	}


	//////////////////////////////////////////////////////////////////////////
	// CanTrigger ( Override )
	public override bool CanTrigger()
	{
		if ( base.CanTrigger() == false )
			return false;

		if ( m_IsDodging == true )
			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// DropEntityDragged
	public	void	DropEntityDragged()
	{
		if ( m_GrabbedObject == null )
			return;

		m_GrabPoint.transform.Translate( 0f, 0f, m_UseDistance );

		Rigidbody rb		= m_GrabbedObject.RigidBody;
		rb.useGravity		= m_GrabbedObjectUseGravity;
		rb.mass				= m_GrabbedObjectMass;

		Physics.IgnoreCollision( m_PhysicCollider, m_GrabbedObject.Collider, ignore: false );

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
		rb.rotation = CameraControl.Instance.Transform.rotation;
		rb.angularVelocity = Vector3.zero;
		rb.velocity = ( m_GrabPoint.transform.position - m_GrabbedObject.transform.position ) / ( Time.fixedDeltaTime * 4f );
//		* ( 1.0f - Vector3.Angle( transform.forward, CameraControl.Instance.transform.forward ) / CameraControl.CLAMP_MAX_X_AXIS );
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
			m_Move *= GroundSpeedModifier * Time.timeScale;
			m_RigidBody.velocity = m_Move;
			return;
		}

		// User inputs
		if ( IsGrounded )
		{
			Vector3 forward = Vector3.Cross( CameraControl.Instance.Transform.right, transform.up );
			m_Move = ( m_MoveSmooth * forward ) + ( m_StrafeSmooth * CameraControl.Instance.Transform.right );

			m_Move *= GroundSpeedModifier;
			m_RigidBody.velocity = m_Move;
		}
		else
		{
			// add gravity force
			m_RigidBody.AddForce( transform.up * Physics.gravity.y, ForceMode.Acceleration );
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		this.OnFrame( Time.deltaTime );
	}
	*/

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

		Grabbable grabbable = null;

		// Distance check
		if ( m_CanGrabObjects == true && m_RaycastHit.distance <= m_UseDistance )
		{
			grabbable = m_RaycastHit.transform.GetComponent<Grabbable>();
		}

		// ACTION GRAB
		if ( InputManager.Inputs.Use )
		{
			if ( m_GrabbedObject != null )
			{
				DropEntityDragged();
				return;
			}

			if ( grabbable != null && m_Interactable != null && m_Interactable.CanInteract )
			{
				m_GrabbedObject = grabbable;
				m_GrabPoint.transform.localPosition = Vector3.forward * m_RaycastHit.distance;

				Rigidbody rb				= grabbable.RigidBody;
				m_GrabbedObjectMass			= rb.mass;			rb.mass				= 1f;
				m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity		= false;
				rb.velocity					= Vector3.zero;		rb.interpolation	= RigidbodyInterpolation.Interpolate;
				m_CanGrabObjects			= false;

				Physics.IgnoreCollision( m_PhysicCollider, grabbable.Collider, ignore: true );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected override void	OnFrame( float deltaTime )
	{
		if ( m_IsActive == false )
			return;

		// Reset "local" states
		m_States.Reset();

		if ( InputManager.Inputs.Gadget3 && WeaponManager.Instance.CurrentWeapon.FlashLight != null )
		{
			WeaponManager.Instance.CurrentWeapon.FlashLight.Toggle();
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
			Vector3 startLine = CameraControl.Instance.Transform.position;
			Vector3 endLine = CameraControl.Instance.Transform.position + CameraControl.Instance.Transform.forward * MAX_INTERACTION_DISTANCE;

			bool lineCastResult = Physics.Raycast( startLine, endLine - startLine, out m_RaycastHit, MAX_INTERACTION_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore );

			Debug.DrawLine( startLine, endLine );

			CheckForDodge ( lineCastResult );
			CheckForInteraction( lineCastResult );
			CheckForGrab ( lineCastResult );
		}

#endregion

		if ( m_IsDodging == true )
			return;
// Water
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
