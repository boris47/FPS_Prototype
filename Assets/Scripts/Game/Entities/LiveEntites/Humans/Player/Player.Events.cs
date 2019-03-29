﻿
using UnityEngine;

public partial class Player {

	// The ammount of hit effect to show
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
		if ( m_CutsceneManager.IsPlaying == true )
			m_CutsceneManager.Terminate();

		// UI effect reset
		UI.Instance.EffectFrame.color = Color.clear;

		// Dodging reset
		if ( m_DodgeCoroutine != null )
		{
			StopCoroutine( m_DodgeCoroutine );
		}
		m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity						= Vector3.zero;

		GameManager.SetTimeScale( 1.0f );

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

		m_ForwardSmooth = m_RightSmooth = m_UpSmooth = 0f;

		// Health
		m_Health			= streamUnit.GetAsFloat( "Health" );

		// Stamina
		m_Stamina			= streamUnit.GetAsFloat( "Stamina" );

		// Crouch state
		m_States.IsCrouched = streamUnit.GetAsBool( "IsCrouched" );

		// Motion Type
		m_CurrentMotionType	= streamUnit.GetAsEnum<eMotionType>( "MotionType");
		SetMotionType( m_CurrentMotionType );

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
	public		override	void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		m_Health -= damage;
		UI.Instance.InGame.UpdateUI();

		m_DamageEffect = 0.2f;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnThink()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnPhysicFrame( float fixedDeltaTime )
	{
		if ( m_IsActive == false )
			return;

		MoveGrabbedObject();

		m_RigidBody.angularVelocity = Vector3.zero;
//		m_RigidBody.drag = IsGrounded ? BODY_DRAG : 0.0f;

		// Forced by ovverride
		if ( m_MovementOverrideEnabled )
		{
			// Controlled in Player.Motion_Walk::SimulateMovement
//			m_RigidBody.velocity = m_Move;
			m_RigidBody.AddForce( m_Move, ForceMode.Acceleration );
			return;
		}
		
		// User inputs
//		if ( IsGrounded )
		{
			// Controlled in Player.Motion_Walk::Update_Walk
			Vector3 forward	= Vector3.Cross( CameraControl.Instance.Transform.right, transform.up );
			Vector3 right	= CameraControl.Instance.Transform.right;
			Vector3 up		= transform.up;
			
			if ( m_ForwardSmooth != 0.0f )
				m_RigidBody.AddForce( forward	* m_ForwardSmooth	* GroundSpeedModifier,	m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );

			if ( m_RightSmooth != 0.0f )
				m_RigidBody.AddForce( right		* m_RightSmooth		* GroundSpeedModifier,	m_UpSmooth > 0.0f ? ForceMode.Impulse : ForceMode.Acceleration );
				
			if ( m_UpSmooth > 0.0f )
				m_RigidBody.AddForce( up		* m_UpSmooth		* 1.0f,	ForceMode.VelocityChange );

			m_ForwardSmooth = m_RightSmooth = m_UpSmooth = 0.0f;
	//		m_States.IsMoving = false;

			// Reset "local" states
			m_States.Reset();

		}
		
		float drag = IsGrounded ? 5f : 0.0f;
		m_RigidBody.drag = drag;

		// Apply gravity
		{
			// add RELATIVE gravity force
			Vector3 gravity = transform.up * Physics.gravity.y;// * 30;// ( IsGrounded ? 1.0f: 30f );
			m_RigidBody.AddForce( gravity, ForceMode.Acceleration );
		}
	}


	private	bool	GoForwardPredicate()
	{
		return IsGrounded;
	}

	private	void	GoForwardAction()
	{
		float force = 1.0f;
		if ( m_States.IsRunning )
		{
			force	*=	m_RunSpeed;
		}
		else if ( m_States.IsCrouched )
		{
			force	*= m_CrouchSpeed;
		}
		else
		{
			force	*= m_WalkSpeed;
			m_States.IsWalking = true;
		}

		m_States.IsMoving = true;
		m_ForwardSmooth = force;
	}


	private	bool	GoBackwardPredicate()
	{
		return IsGrounded;
	}

	private	void	GoBackwardAction()
	{
		float force = 1.0f;
		if ( m_States.IsRunning )
		{
			force	*=	m_RunSpeed;
		}
		else if ( m_States.IsCrouched )
		{
			force	*= m_CrouchSpeed;
		}
		else
		{
			force	*= m_WalkSpeed;
			m_States.IsWalking = true;
		}

		force *= 0.8f;

		m_States.IsMoving = true;
		m_ForwardSmooth = -force;
	}




	private	bool	StrafeRightPredicate()
	{
		return IsGrounded;
	}

	private	void	StrafeRightAction()
	{
		float force = 1.0f;
		if ( m_States.IsRunning )
		{
			force	*=	m_RunSpeed * 0.8f;
		}
		else if ( m_States.IsCrouched )
		{
			force	*= m_CrouchSpeed * 0.8f;
		}
		else
		{
			force	*= m_WalkSpeed *  0.8f;
			m_States.IsWalking = true;
		}
		m_States.IsMoving = true;
		m_RightSmooth = force;
	}


	private	bool	StrafeLeftPredicate()
	{
		return IsGrounded;
	}

	private	void	StrafeLeftAction()
	{
		float force = 1.0f;
		if ( m_States.IsRunning )
		{
			force	*=	m_RunSpeed * 0.8f;
		}
		else if ( m_States.IsCrouched )
		{
			force	*= m_CrouchSpeed * 0.8f;
		}
		else
		{
			force	*= m_WalkSpeed *  0.8f;
			m_States.IsWalking = true;
		}

		m_States.IsMoving = true;
		m_RightSmooth = -force;
	}


	private	bool	RunPredicate()
	{
		return true;//( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched );
	}

	private	void	RunAction()
	{
		m_States.IsCrouched = false;
		m_States.IsRunning = true;
	}


	private	bool	JumpPredicate()
	{
		return IsGrounded && m_States.IsJumping == false && m_States.IsHanging == false && m_States.IsFalling == false && m_GrabbedObject == null;
	}

	private	void	JumpAction()
	{
		m_UpSmooth = m_JumpForce / ( m_States.IsCrouched ? 1.5f : 1.0f );
		m_States.IsJumping = true;
	}


	private	bool FlashlightUsagePredicate()
	{
		return WeaponManager.Instance.CurrentWeapon.Flashlight != null;
	}


	private	void FlashlightUsageAction()
	{
		WeaponManager.Instance.CurrentWeapon.Flashlight.Toggle();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnFrame( float deltaTime )
	{
		if ( m_IsActive == false )
			return;
		
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
			Vector3 position  = CameraControl.Instance.Transform.position;
			Vector3 direction = CameraControl.Instance.Transform.forward;
			bool lineCastResult = Physics.Raycast( position, direction, out m_RaycastHit, MAX_INTERACTION_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore );

//			Vector3 startLine = CameraControl.Instance.Transform.position;
//			Vector3 endLine   = CameraControl.Instance.Transform.position + CameraControl.Instance.Transform.forward * MAX_INTERACTION_DISTANCE;
//			Debug.DrawLine( startLine, endLine );

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
		CameraControl.Instance.Transform.SetParent( null );

		m_IsActive = false;

		// reset effect
		var settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		settings.intensity = 0f;
		CameraControl.Instance.GetPP_Profile.vignette.settings = settings;

		// disable weapon actions
		WeaponManager.Instance.CurrentWeapon.Enabled = false;
		WeaponManager.Instance.Enabled = false;
		
		
		// Disable camera updates
		CameraControl.Instance.Enabled = false;

		// Update UI elements
		UI.Instance.InGame.UpdateUI();

		// Turn off player object
		gameObject.SetActive( false );

		// print a message
		print( "U r dead" );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		OnDestroy()
	{
		if ( m_DodgeAbilityTarget != null )
			Destroy( m_DodgeAbilityTarget.gameObject );

		Instance = null;
		Entity = null;
	}
}