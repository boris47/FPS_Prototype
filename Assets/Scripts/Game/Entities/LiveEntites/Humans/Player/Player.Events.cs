
using UnityEngine;

public partial class Player {

	private	const	float			MAX_INTERACTION_DISTANCE		= 40.1f; // TODO set to 2.1

	// The ammount of hit effect to show

	[Header("Player Events")]
	[SerializeField]
	private		float				m_DamageEffect					= 0f;


	//////////////////////////////////////////////////////////////////////////
	protected	override	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= base.OnSave( streamData );
		if ( streamUnit == null )
			return null;

		// Health
		streamUnit.SetInternal( "Health", m_Health );

		// Stamina
		streamUnit.SetInternal( "Stamina", m_Stamina );

		// Crouch state
		streamUnit.SetInternal( "IsCrouched", IsCrouched );

		// Motion Type
		streamUnit.SetInternal( "MotionType", m_CurrentMotionType );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		// Cutscene Manager
		if (m_CutsceneManager.IsPlaying == true )
			m_CutsceneManager.Terminate();

		// UI effect reset
		UIManager.EffectFrame.color = Color.clear;

		// Dodging reset
//		if (this.m_DodgeCoroutine != null )
//		{
//			this.StopCoroutine(this.m_DodgeCoroutine );
//		}
		m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity						= Vector3.zero;

		GlobalManager.SetTimeScale( 1.0f );

//		this.m_DodgeRaycastNormal						= Vector3.zero;
//		this.m_DodgeAbilityTarget.gameObject.SetActive( false );
//		this.m_ChosingDodgeRotation						= false;
//		this.m_DodgeInterpolant							= 0f;
		UnityEngine.PostProcessing.MotionBlurModel.Settings settings								= CameraControl.Instance.PP_Profile.motionBlur.settings;
		settings.frameBlending						= 0f;
		CameraControl.Instance.PP_Profile.motionBlur.settings = settings;
//		this.m_IsDodging = false;

		// Player internals
		m_Interactable								= null;
		m_Move										= Vector3.zero;
		m_RaycastHit								= default( RaycastHit );

		DropEntityDragged();

		m_ForwardSmooth = m_RightSmooth = m_UpSmooth = 0f;

		// Health
		m_Health			= streamUnit.GetAsFloat( "Health" );

		// Stamina
		m_Stamina			= streamUnit.GetAsFloat( "Stamina" );

		// Crouch state
		m_States.IsCrouched = streamUnit.GetAsBool( "IsCrouched" );

		// Motion Type
		m_CurrentMotionType	= streamUnit.GetAsEnum<EMotionType>( "MotionType");
		SetMotionType(m_CurrentMotionType );

		m_RigidBody.useGravity = false;

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetAquired( TargetInfo targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetChanged( TargetInfo targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnTargetLost( TargetInfo targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void		OnHittedDetails( Vector3 startPosition, Entity whoRef, EDamageType damageType, float damage, bool canPenetrate = false )
	{
		m_DamageEffect = 0.2f; // damage / m_Health;

		m_Health -= damage;
		UIManager.InGame.UpdateUI();

		if (m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnThink()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnPhysicFrame( float fixedDeltaTime )
	{
		if (m_IsActive == false )
			return;

		MoveGrabbedObject();
		CheckIfUnderSomething();
		//		CheckForFallOrUserBreak();

		m_RigidBody.angularVelocity = Vector3.zero;

		// Forced by ovverride
		if (m_MovementOverrideEnabled )
		{
			// Controlled in Player.Motion_Walk::SimulateMovement
			m_RigidBody.AddForce(m_Move, ForceMode.Acceleration );
			return;
		}
		
		// Apply User inputs
		{
			// Controlled in Player.Motion_Walk::Update_Walk
			Vector3 forward	= Vector3.Cross( CameraControl.Instance.transform.right, transform.up );
			Vector3 right	= CameraControl.Instance.transform.right;
			Vector3 up		= transform.up;
			
			if ( !Utils.Math.SimilarZero( m_ForwardSmooth, 0.01f ))//  this.m_ForwardSmooth != 0.0f )
				m_RigidBody.AddForce( forward	* m_ForwardSmooth	* GroundSpeedModifier, m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );

	//		if (this.m_RightSmooth != 0.0f )
			if (!Utils.Math.SimilarZero(m_RightSmooth, 0.01f))
				m_RigidBody.AddForce( right		* m_RightSmooth		* GroundSpeedModifier, m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );
				
	//		if (this.m_UpSmooth > 0.0f )
			if (!Utils.Math.SimilarZero(m_UpSmooth, 0.01f))
				m_RigidBody.AddForce( up		* m_UpSmooth		* 1.0f,	ForceMode.VelocityChange );

			m_ForwardSmooth = m_RightSmooth = m_UpSmooth = 0.0f;

			// Reset "local" states
			m_States.Reset();
		}
		
		float drag = IsGrounded ? 7f : 0.0f;
		m_RigidBody.drag = drag;

		// Apply gravity
		{
			// add RELATIVE gravity force
			Vector3 gravity = transform.up * Physics.gravity.y;
			m_RigidBody.AddForce( gravity, ForceMode.Acceleration );
		}
	}
	


	// Pick eventual collision info from camera to up
	private	void	CheckIfUnderSomething()
	{
		Vector3 position = CameraControl.Instance.transform.position;
		Vector3 upwards = CameraControl.Instance.transform.up;
		Vector3 cameraUpPosition = position + ( upwards * 0.3f );
		m_IsUnderSomething = Physics.Linecast( start: position, end: cameraUpPosition, layerMask: Physics.DefaultRaycastLayers, queryTriggerInteraction: QueryTriggerInteraction.Ignore );
	}



	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
		if (m_IsActive == false )
			return;

		// Damage Effect
		UnityEngine.PostProcessing.VignetteModel.Settings settings = CameraControl.Instance.PP_Profile.vignette.settings;
		if (m_DamageEffect > 0.0f )
		{
			m_DamageEffect = Mathf.Lerp(m_DamageEffect, 0f, Time.deltaTime * 2f );
			settings.intensity = m_DamageEffect;
			CameraControl.Instance.PP_Profile.vignette.settings = settings;
		}
		

		////////////////////////////////////////////////////////////////////////////////////////
		// Check for usage
#region		INTERACTIONS
		{
			Vector3 position  = CameraControl.Instance.transform.position;
			Vector3 direction = CameraControl.Instance.transform.forward;
			m_HasRaycasthit = Physics.Raycast( position, direction, out m_RaycastHit, MAX_INTERACTION_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore );
		}

#endregion

//		if (this.m_IsDodging == true )
//			return;
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
//			switch ( m_CurrentMotionType )
//			{
//				case eMotionType.Walking:	{ this.Update_Walk();		break; }
//				case eMotionType.Flying:	{ this.Update_Fly();		break; }
//				case eMotionType.Swimming:	{ this.Update_Swim();		break; }
//				case eMotionType.Swimming:	{ this->Update_Swim( bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater );	break; }
//				case eMotionType.P1ToP2:	{ this.Update_P1ToP2();		break; }
//			}
		}

		// trace previuos states
		m_PreviousStates = m_States;
	}


	//////////////////////////////////////////////////////////////////////////
	protected		override	void		OnKill()
	{
		// remove parent for camera
		CameraControl.Instance.transform.SetParent( null );

		m_IsActive = false;

		// reset effect
		UnityEngine.PostProcessing.VignetteModel.Settings settings = CameraControl.Instance.PP_Profile.vignette.settings;
		settings.intensity = 0f;
		CameraControl.Instance.PP_Profile.vignette.settings = settings;

		// disable weapon actions
		WeaponManager.Instance.CurrentWeapon.Enabled = false;
		WeaponManager.Instance.Enabled = false;
		
		
		// Disable camera updates
		CameraControl.Instance.enabled = false;

		// Update UI elements
		UIManager.InGame.UpdateUI();

		// Turn off player object
		gameObject.SetActive( false );

		// print a message
		print( "U r dead" );
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnDestroy()
	{
		if ( m_DodgeAbilityTarget != null )
			Destroy( m_DodgeAbilityTarget.gameObject );

		m_Instance = null;
		Entity = null;
	}
	*/
}