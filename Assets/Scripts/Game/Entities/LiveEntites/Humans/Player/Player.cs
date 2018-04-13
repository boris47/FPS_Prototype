
using UnityEngine;


public partial class Player : Human {

	[Header("Player Properties")]

	private	const	float			MAX_INTERACTION_DISTANCE		= 20f;

	public	static	Player			Instance						= null;

	// DASHING
	private		bool				m_IsDashing						= false;
	public		bool				IsDashing
	{
		get { return m_IsDashing; }
	}
	private		DashTarget			m_CurrentDashTarget				= null;
	private		DashTarget			m_PreviousDashTarget			= null;

	private		Grabbable			m_Grabbable						= null;
	private		IInteractable		m_Interactable					= null;

	private		Vector3				m_Move							= Vector3.zero;

	private		ITourchLight		m_TourchLight					= null;

	private		RaycastHit			m_RaycastHit;



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
		DontDestroyOnLoad( this );

		base.Awake();

		// Player Components
		{
			// TourchLight
			m_TourchLight	= transform.Find("TourchLight").GetComponent<ITourchLight>();
			// Foots
			m_Foots			= transform.Find( "FootSpace" ).GetComponent<IFoots>();
		}

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
		m_Stamina			= 1.0f;
		GroundSpeedModifier = 1.0f;
		m_IsActive			= true;

		SetMotionType( eMotionType.Walking );


		m_GrabPoint = new GameObject( "GrabPoint" );
		m_GrabPoint.transform.SetParent( transform );
		m_GrabPoint.transform.localPosition = Vector3.zero;
		m_GrabPoint.transform.localRotation = Quaternion.identity;
		m_GrabPoint.transform.Translate( 0f, 0f, m_UseDistance );

		CameraControl.Instance.transform.rotation = transform.rotation;

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
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		this.OnFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame
	public override void	OnFrame( float deltaTime )
	{
		if ( m_IsActive == false || m_IsDashing == true )
			return;

		// Reset "local" states
		m_States.Reset();

		if ( InputManager.Inputs.ItemAction3 )
		{
			m_TourchLight.Toggle();
		}

		// Weapon switch
		if ( InputManager.Inputs.SwitchPrev )
		{
			WeaponManager.Instance.ChangeWeapon( -1 );
		}
		if ( InputManager.Inputs.SwitchNext )
		{
			WeaponManager.Instance.ChangeWeapon( 1 );
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
#region			INTERACTIONS
		{
			// Get interactable / draggable object
			if ( m_GrabbedObject == null )
			{
				Debug.DrawLine(
					CameraControl.Instance.transform.position, 
					CameraControl.Instance.transform.position + CameraControl.Instance.transform.forward * MAX_INTERACTION_DISTANCE
				);

				if ( Physics.Raycast( CameraControl.Instance.transform.position, CameraControl.Instance.transform.forward, out m_RaycastHit, MAX_INTERACTION_DISTANCE ) )
				{
//					if ( m_IsDashing == false )
//					{
						// Only if needed
						DashTarget currentTarget = m_RaycastHit.transform.GetComponent<DashTarget>();
						if ( currentTarget != m_CurrentDashTarget )
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
//					}

					if ( m_CanGrabObjects == true && m_RaycastHit.distance <= m_UseDistance )
					{
						m_Grabbable		= m_RaycastHit.transform.GetComponent<Grabbable>();
						m_Interactable	= m_RaycastHit.transform.GetComponent<IInteractable>();
					}
				}
				else
				{
					if ( m_CurrentDashTarget != null )
					{
						m_CurrentDashTarget.HideText();
						m_CurrentDashTarget		= null;
					}
					m_Grabbable				= null;
					m_Interactable			= null;
				}
			}
			else
			{
				// Update Grab point position
				m_GrabPoint.transform.position = CameraControl.Instance.transform.position + ( CameraControl.Instance.transform.forward * m_UseDistance );
				m_GrabPoint.transform.rotation = CameraControl.Instance.transform.rotation;
			}
			

			if ( InputManager.Inputs.Use )
			{
				if ( m_GrabbedObject == null )
				{
					if ( m_CurrentDashTarget != null )
					{
						OnDashTargetUsed( ref m_CurrentDashTarget );
						return;
					}

					// Interaction
					if ( m_Interactable != null && m_Interactable.CanInteract )
						m_Interactable.OnInteraction();

					// Grab
					if ( m_Grabbable != null && m_Interactable.CanInteract )
					{
						m_GrabbedObject = m_RaycastHit.transform.gameObject;

						Rigidbody rb = m_GrabbedObject.GetComponent<Rigidbody>();
						m_GrabbedObjectMass			= rb.mass;			rb.mass = 1f;
						m_GrabbedObjectUseGravity	= rb.useGravity;	rb.useGravity = false;
						rb.interpolation = RigidbodyInterpolation.Extrapolate;
						m_CanGrabObjects = false;
					}
				}
				else
				{
					DropEntityDragged();
				}
			}

/*			if ( Inputmanager.Inputs.Item1 )
			{
				if ( m_GrabbedObject != null )
				{
					m_GrabbedObject.GetComponent<Rigidbody>().velocity = ( CameraControl.Instance.transform.forward * 10f );
					DropEntityDragged();
				}
			}
*/		}

#endregion

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

		// rotate the capsule of the player
		transform.rotation = Quaternion.Euler( Vector3.Scale( CameraControl.Instance.transform.rotation.eulerAngles, Vector3.up ) );

		if ( WeaponManager.Instance.CurrentWeapon.FirePoint1 != null )
		{
			m_TourchLight.Transform.position = WeaponManager.Instance.CurrentWeapon.FirePoint1.position;
			m_TourchLight.Transform.forward = WeaponManager.Instance.CurrentWeapon.FirePoint1.forward;
		}


		// Update flashlight position and rotation
//		pFlashLight->Update();

		// trace previuos states
		m_PreviousStates = m_States;

	}

	public override void OnThink()
	{
		
	}

}
