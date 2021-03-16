
using UnityEngine;


public partial class Player
{
	RigidbodyInterpolation prevInterpolation;

	//////////////////////////////////////////////////////////////////////////
	public	override	void		EnterSimulationState()
	{
		GlobalManager.InputMgr.DisableCategory(EInputCategory.ALL);
		Interactions.DropGrabbedObject();

		m_MovementOverrideEnabled				= true;
		m_SimulationStartPosition				= transform.position;
		prevInterpolation						= m_RigidBody.interpolation;
		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override		void		BeforeSimulationStage( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		m_SimulationStartPosition = transform.position;
		FPSEntityCamera.Instance.SetTarget(target);
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool		SimulateMovement( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// END OF SIMULATION STEP
		Vector3 direction = (destination - m_SimulationStartPosition);
		float simulationdDistanceToTravel = direction.sqrMagnitude;
		float simulationDistanceTravelled = (transform.position - m_SimulationStartPosition).sqrMagnitude;

		if (simulationDistanceTravelled > simulationdDistanceToTravel)
		{
			Motion.MotionStrategy.Move(EMovementType.STATIONARY, Vector3.zero);
			return false;				// force logic update
		}

		float interpolant = simulationDistanceTravelled / simulationdDistanceToTravel;

		// TIME SCALE
		float timeScale = Mathf.Lerp(Time.timeScale, timeScaleTarget, interpolant);

		SoundManager.Pitch = Time.timeScale = Mathf.Clamp01(timeScale);

		FPSEntityCamera.Instance.SetTarget(target);

		Motion.MotionStrategy.States.IsWalking	= movementType != EMovementType.RUN;
		Motion.MotionStrategy.States.IsRunning	= !Motion.MotionStrategy.States.IsWalking;
		Motion.MotionStrategy.States.IsCrouched	= movementType == EMovementType.CROUCHED;
		Motion.MotionStrategy.States.IsMoving	= movementType != EMovementType.STATIONARY;
		Motion.MotionStrategy.Move(movementType, direction.normalized);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	 override		void		AfterSimulationStage( EMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		m_SimulationStartPosition = Vector3.zero;

		FPSEntityCamera.Instance.SetTarget(target);

		Motion.MotionStrategy.Move(EMovementType.STATIONARY, Vector3.zero);
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	void		ExitSimulationState()
	{
		m_MovementOverrideEnabled				= false;
		m_SimulationStartPosition				= Vector3.zero;

		m_RigidBody.interpolation				= prevInterpolation;

		Motion.MotionStrategy.States.IsWalking		= false;
		Motion.MotionStrategy.States.IsRunning		= false;
		Motion.MotionStrategy.States.IsCrouched		= false;
		Motion.MotionStrategy.States.IsMoving		= false;

		GlobalManager.SetTimeScale(1f);

		GlobalManager.InputMgr.EnableCategory(EInputCategory.ALL);

		FPSEntityCamera.Instance.SetTarget(null);

		Motion.MotionStrategy.Move(EMovementType.STATIONARY, Vector3.zero);
	}


//	//////////////////////////////////////////////////////////////////////////
//	private					void		Update_Walk()
//	{
//		if ( m_Health <= 0.0f )
//			return;

//		if ( m_MovementOverrideEnabled == true )
//			return;

//		float 	forward 		= InputManager.Inputs.Forward	  ? 1.0f : InputManager.Inputs.Backward   ? -1.0f : 0.0f;
//		float 	right			= InputManager.Inputs.StrafeRight ? 1.0f : InputManager.Inputs.StrafeLeft ? -1.0f : 0.0f;
//		bool 	bSprintInput	= InputManager.Inputs.Run;
//		bool	bCrouchInput	= InputManager.Inputs.Crouch;
//		bool	bJumpInput		= InputManager.Inputs.Jump;

//		float	fFinalJump		= 0.0f;
		
//		if ( InputManager.HoldCrouch == true )
//			m_States.IsCrouched = m_PreviousStates.IsCrouched;

		
//		if ( IsGrounded == false && bJumpInput == false )
//		{

//			float verticalSpeed = transform.InverseTransformDirection( m_RigidBody.velocity ).y;
//			if ( verticalSpeed >  0.01f )
//			{
//				m_States.IsHanging = true;
//				m_Foots.Collider.enabled = false;
//			}
//			if ( verticalSpeed < -0.01f )
//			{
//				m_States.IsFalling = true;
//				m_Foots.Collider.enabled = true;
//			}


//			/*
//			if ( m_PreviousStates.IsHanging && m_States.IsFalling )
//				m_LastLandTime = Time.time;


//			float iFallTime = Time.time - m_LastLandTime;
//			if ( iFallTime > 100 )
//				m_Landed = false;
//			if ( iFallTime > 400 )
//				m_HeavyFall = true;
//				*/
////			m_Stamina = Mathf.Min( m_Stamina + ( m_StaminaRestore * Time.deltaTime ), 1.0f );

//			return;
//		}



//		////////////////////////////////////////////////////////////////////////////////////////
//		// Play step sound when lands over terrain
//		{

//		}

		   
//		////////////////////////////////////////////////////////////////////////////////////////
//		// Crouch State
//		{
//			// If crouch button is pressed
//			if ( bCrouchInput )
//			{
//				m_States.IsCrouched = m_PreviousStates.IsCrouched;



//				/*
//				m_Collider.center = new Vector3
//				(
//					m_Collider.center.x,
//					m_States.IsCrouched ? ( -m_Collider.height * 0.5f ): 0.0f,
//					m_Collider.center.z
//				);

//				m_Collider.height *= m_States.IsCrouched ? 0.5f : 2.0f;
//				*/


//				// adapt the collider

//				/*
//				// If is crouched
//				if ( m_States.IsCrouched )
//				{
//					if ( !m_IsUnderSomething )
//					{
//						CameraControl.Instance.HeadBob.Reset( true );
//						m_States.IsCrouched = false;
//					}
//					else
//					{
//						if ( bJumpInput ) fFinalJump = m_JumpForce / 2.0f;
//						CameraControl.Instance.HeadBob.Reset( true );
//						m_States.IsCrouched = false;
//					}
//				}
//				else
//				{
//					m_States.IsCrouched = true;
//				}
//				*/
//			}
//		}


//		////////////////////////////////////////////////////////////////////////////////////////
//		// Jump
//		{
//			// If jump button is pressed, has enough space to jump, has stamina requirements and is not dragging an entity
//			if ( bJumpInput && !m_IsUnderSomething && ( m_Stamina > m_StaminaJumpMin ) && m_GrabbedObject == null && IsGrounded )
//			{
/////				m_Stamina	-= m_JumpStamina;
//				fFinalJump	+= m_JumpForce / ( m_States.IsCrouched ? 1.5f : 1.0f );
////				fFinalJump	*= IsInWater() ? 0.8f : 1.0f;
//				m_States.IsJumping = true;
//			}
//		}


//		////////////////////////////////////////////////////////////////////////////////////////
//		// Run State
//		{

//			// When stamina reach zero, player became tired
//			if ( m_Stamina <= 0.0 ) m_Tiredness = true;

//			// When player is tired and stamina overcome a defined value player is no more tired
//			if ( m_Tiredness && m_Stamina > m_StaminaRunMin ) m_Tiredness = false;

//			// So if run button is pressed, player is not tired and if crouched can stan up
//			if ( bSprintInput && !m_Tiredness && ( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched ) ) {

//		//		if ( IsDragging() ) DropEntityDragged();

////				CamManager()->GetZoomEffect()->Reset();

//				m_States.IsCrouched = false;
//				m_States.IsRunning = true;

//			}

//		}

//		float dt = Time.deltaTime;

//		////////////////////////////////////////////////////////////////////////////////////////
//		// Process inputs
//		// If receive input for moving
//		if ( ( forward != 0.0 ) || ( right != 0.0 ) )
//		{
//			if ( m_States.IsRunning )
//			{
//				forward		*=	m_RunSpeed * ( forward > 0 ? 1.0f : 0.8f );
//				right		*=	m_RunSpeed * 0.8f;
/////				m_Stamina	-= m_RunStamina * dt;
//			}
//			else if ( m_States.IsCrouched )
//			{
//				forward		*= m_CrouchSpeed * ( forward > 0 ? 1.0f : 0.8f );
//				right		*= m_CrouchSpeed * 0.8f;
/////				m_Stamina	-= m_CrouchStamina * dt;
//			}
//			else
//			{	// walking
//				// stamina half restored because we are moving, but just walking
//				forward		*= m_WalkSpeed * ( forward > 0 ? 1.0f : 0.8f );
//				right		*= m_WalkSpeed *  0.8f;
/////				m_Stamina	+= m_StaminaRestore / 2.0f * dt;
//				m_States.IsWalking = true;
//			}

//			if ( m_States.IsJumping == false )
//			{
//				m_States.IsMoving = true;
//			}
//		}

//		// if don't move stamina regenerates at regular speed
//		if ( forward == 0.0f && right == 0.0f )
//			m_Stamina += m_StaminaRestore * dt;

//		// Clamp Stamina between 0.0 and 1.0
//		m_Stamina = Mathf.Clamp( m_Stamina, 0.0f, 1.0f );

//		// boost movements when Jumping
//		if ( m_States.IsJumping )
//		{
//			if ( m_States.IsWalking  )		{ forward *= m_WalkJumpCoef;	right *= m_WalkJumpCoef;	fFinalJump *= m_WalkJumpCoef; }
//			if ( m_States.IsRunning  )		{ forward *= m_RunJumpCoef;		right *= m_RunJumpCoef;		fFinalJump *= m_RunJumpCoef; }
//			if ( m_States.IsCrouched )		{ fFinalJump *= m_CrouchJumpCoef; }
//		}

//		// Apply smoothing on movements
////		m_ForwardSmooth	= forward; // Mathf.Lerp( m_ForwardSmooth,  forward,	dt * 20f );
////		m_RightSmooth	= right; // Mathf.Lerp( m_RightSmooth,	right,		dt * 10f );
////		m_UpSmooth		= fFinalJump;
//		/*
//		m_Move = ( m_MoveSmooth * transform.forward ) + ( m_StrafeSmooth * transform.right );

//		// This prevents "speed hack" strafing
//		if ( ( fStrafe != 0.0f ) && ( fMove != 0.0f  ) )
//		{
//			m_Move *= 0.707f;
//		}

//		// Apply ground speed modifier
//		m_Move *= GroundSpeedModifier;
//		*/
		
////		m_RigidBody.useGravity = false;

//		// Update internal time value
//		// Used for timed operation such as high jump or other things
//		m_LastLandTime = Time.time;

//	}
// /
	

}
