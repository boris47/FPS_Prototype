
using UnityEngine;

public partial class Player {

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
		streamUnit.SetInternal( "Health", this.m_Health );

		// Stamina
		streamUnit.SetInternal( "Stamina", this.m_Stamina );

		// Crouch state
		streamUnit.SetInternal( "IsCrouched", this.IsCrouched );

		// Motion Type
		streamUnit.SetInternal( "MotionType", this.m_CurrentMotionType );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		// Cutscene Manager
		if (this.m_CutsceneManager.IsPlaying == true )
			this.m_CutsceneManager.Terminate();

		// UI effect reset
		UIManager.EffectFrame.color = Color.clear;

		// Dodging reset
		if (this.m_DodgeCoroutine != null )
		{
			this.StopCoroutine(this.m_DodgeCoroutine );
		}
		this.m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		this.m_RigidBody.velocity						= Vector3.zero;

		GlobalManager.SetTimeScale( 1.0f );

		this.m_DodgeRaycastNormal						= Vector3.zero;
		this.m_DodgeAbilityTarget.gameObject.SetActive( false );
		this.m_ChosingDodgeRotation						= false;
		this.m_DodgeInterpolant							= 0f;
		UnityEngine.PostProcessing.MotionBlurModel.Settings settings								= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		settings.frameBlending						= 0f;
		CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;
		this.m_IsDodging = false;

		// Player internals
		this.m_Interactable								= null;
		this.m_Move										= Vector3.zero;
		this.m_RaycastHit								= default( RaycastHit );

		this.DropEntityDragged();

		this.m_ForwardSmooth = this.m_RightSmooth = this.m_UpSmooth = 0f;

		// Health
		this.m_Health			= streamUnit.GetAsFloat( "Health" );

		// Stamina
		this.m_Stamina			= streamUnit.GetAsFloat( "Stamina" );

		// Crouch state
		this.m_States.IsCrouched = streamUnit.GetAsBool( "IsCrouched" );

		// Motion Type
		this.m_CurrentMotionType	= streamUnit.GetAsEnum<EMotionType>( "MotionType");
		this.SetMotionType(this.m_CurrentMotionType );

		this.m_RigidBody.useGravity = false;

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
		this.m_DamageEffect = 0.2f; // damage / m_Health;

		this.m_Health -= damage;
		UIManager.InGame.UpdateUI();

		if (this.m_Health < 0f )
			this.OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnThink()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnPhysicFrame( float fixedDeltaTime )
	{
		if (this.m_IsActive == false )
			return;

		this.MoveGrabbedObject();
		this.CheckIfUnderSomething();
		//		CheckForFallOrUserBreak();

		this.m_RigidBody.angularVelocity = Vector3.zero;

		// Forced by ovverride
		if (this.m_MovementOverrideEnabled )
		{
			// Controlled in Player.Motion_Walk::SimulateMovement
			this.m_RigidBody.AddForce(this.m_Move, ForceMode.Acceleration );
			return;
		}
		
		// Apply User inputs
		{
			// Controlled in Player.Motion_Walk::Update_Walk
			Vector3 forward	= Vector3.Cross( CameraControl.Instance.Transform.right, this.transform.up );
			Vector3 right	= CameraControl.Instance.Transform.right;
			Vector3 up		= this.transform.up;
			
			if ( !Utils.Math.SimilarZero( this.m_ForwardSmooth, 0.01f ))//  this.m_ForwardSmooth != 0.0f )
				this.m_RigidBody.AddForce( forward	* this.m_ForwardSmooth	* this.GroundSpeedModifier, this.m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );

	//		if (this.m_RightSmooth != 0.0f )
			if (!Utils.Math.SimilarZero(this.m_RightSmooth, 0.01f))
				this.m_RigidBody.AddForce( right		* this.m_RightSmooth		* this.GroundSpeedModifier, this.m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );
				
	//		if (this.m_UpSmooth > 0.0f )
			if (!Utils.Math.SimilarZero(this.m_UpSmooth, 0.01f))
				this.m_RigidBody.AddForce( up		* this.m_UpSmooth		* 1.0f,	ForceMode.VelocityChange );

			this.m_ForwardSmooth = this.m_RightSmooth = this.m_UpSmooth = 0.0f;

			// Reset "local" states
			this.m_States.Reset();
		}
		
		float drag = this.IsGrounded ? 7f : 0.0f;
		this.m_RigidBody.drag = drag;

		// Apply gravity
		{
			// add RELATIVE gravity force
			Vector3 gravity = this.transform.up * Physics.gravity.y;
			this.m_RigidBody.AddForce( gravity, ForceMode.Acceleration );
		}
	}



	private	bool FlashlightPredicate()
	{
		return WeaponManager.Instance.CurrentWeapon?.Flashlight != null;
	}



	private	void FlashlightAction()
	{
		WeaponManager.Instance.CurrentWeapon.Flashlight.SetActive( !WeaponManager.Instance.CurrentWeapon.Flashlight.IsActive );
	}
	


	// Pick eventual collision info from camera to up
	private	void	CheckIfUnderSomething()
	{
		Vector3 position = CameraControl.Instance.Transform.position;
		Vector3 upwards = CameraControl.Instance.Transform.up;
		Vector3 cameraUpPosition = position + ( upwards * 0.3f );
		this.m_IsUnderSomething = Physics.Linecast( start: position, end: cameraUpPosition, layerMask: Physics.DefaultRaycastLayers, queryTriggerInteraction: QueryTriggerInteraction.Ignore );
	}



	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
		if (this.m_IsActive == false )
			return;

		// Damage Effect
		UnityEngine.PostProcessing.VignetteModel.Settings settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		if (this.m_DamageEffect > 0.0f )
		{
			this.m_DamageEffect = Mathf.Lerp(this.m_DamageEffect, 0f, Time.deltaTime * 2f );
			settings.intensity = this.m_DamageEffect;
			CameraControl.Instance.GetPP_Profile.vignette.settings = settings;
		}
		

		////////////////////////////////////////////////////////////////////////////////////////
		// Check for usage
#region		INTERACTIONS
		{
			Vector3 position  = CameraControl.Instance.Transform.position;
			Vector3 direction = CameraControl.Instance.Transform.forward;
			this.m_HasRaycasthit = Physics.Raycast( position, direction, out this.m_RaycastHit, MAX_INTERACTION_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore );
		}

#endregion

		if (this.m_IsDodging == true )
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
		this.m_PreviousStates = this.m_States;
	}


	//////////////////////////////////////////////////////////////////////////
	protected		override	void		OnKill()
	{
		// remove parent for camera
		CameraControl.Instance.Transform.SetParent( null );

		this.m_IsActive = false;

		// reset effect
		UnityEngine.PostProcessing.VignetteModel.Settings settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		settings.intensity = 0f;
		CameraControl.Instance.GetPP_Profile.vignette.settings = settings;

		// disable weapon actions
		WeaponManager.Instance.CurrentWeapon.Enabled = false;
		WeaponManager.Instance.Enabled = false;
		
		
		// Disable camera updates
		CameraControl.Instance.Enabled = false;

		// Update UI elements
		UIManager.InGame.UpdateUI();

		// Turn off player object
		this.gameObject.SetActive( false );

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