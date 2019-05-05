
using UnityEngine;


public partial class Player {

	RigidbodyInterpolation prevInterpolation;

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		m_MovementOverrideEnabled				= true;
		m_SimulationStartPosition				= transform.position;

		CameraControl.Instance.CanParseInput	= false;
		InputManager.IsEnabled					= false;

		prevInterpolation						= m_RigidBody.interpolation;
		m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;

		DropEntityDragged();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override		void		BeforeSimulationStage( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		CameraControl.Instance.Target = target;

		base.BeforeSimulationStage( movementType, destination, target, timeScaleTarget );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// END OF SIMULATION STEP
		Vector3 direction = ( destination - m_SimulationStartPosition );
		float simulationdDistanceToTravel = direction.sqrMagnitude;
		float simulationDistanceTravelled = ( transform.position - m_SimulationStartPosition ).sqrMagnitude;
		
		if ( simulationDistanceTravelled > simulationdDistanceToTravel )
		{
			m_SimulationStartPosition = transform.position;
			return false;				// force logic update
		}

		float interpolant = simulationDistanceTravelled / simulationdDistanceToTravel;

		// TIME SCALE
		float timeScale = Mathf.Lerp( Time.timeScale, timeScaleTarget, interpolant );
		Time.timeScale = Mathf.Clamp01( timeScale );
		SoundManager.Instance.Pitch = timeScale;
		
		//	POSITION BY DISTANCE
		{
			bool isWalking	= ( movementType != SimMovementType.RUN );
			bool isRunning	= !isWalking;
			bool isCrouched = ( movementType == SimMovementType.CROUCHED );
			m_ForwardSmooth = ( movementType != SimMovementType.STATIONARY ) ? ( isCrouched ) ? m_CrouchSpeed : ( isRunning ) ? m_RunSpeed : m_WalkSpeed : 0.0f;

			m_States.IsWalking	= isWalking;
			m_States.IsRunning	= isRunning;
			m_States.IsCrouched	= isCrouched;
			m_States.IsMoving	= movementType != SimMovementType.STATIONARY;

			m_Move = ( m_ForwardSmooth * direction.normalized ) * GroundSpeedModifier;
//			m_RigidBody.velocity = m_Move;
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override		void		AfterSimulationStage( SimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		base.AfterSimulationStage( movementType, destination, target, timeScaleTarget );
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		m_MovementOverrideEnabled = false;
		m_SimulationStartPosition = Vector3.zero;

		m_RigidBody.interpolation = prevInterpolation;

		if ( CameraControl.Instance.Target != null )
		{
			Vector3 projectedTarget = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, CameraControl.Instance.Target.position );
			Quaternion rotation = Quaternion.LookRotation( projectedTarget - transform.position, transform.up );
			transform.rotation = rotation;
		}

		m_Move					= Vector3.zero;
		m_ForwardSmooth			= 0f;
		m_States.IsWalking		= false;
		m_States.IsRunning		= false;
		m_States.IsMoving		= false;
		Time.timeScale			= 1f;

		CameraControl.Instance.CanParseInput = true;
		InputManager.IsEnabled = true;
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
	
		
	/* MOTION TYPE GROUNDED */

	private	void	RegisterGroundedMotion()
	{
		GameManager.Instance.InputMgr.BindCall( eInputCommands.MOVE_FORWARD,	"ForwardEvent",		GoForwardAction,		Motion_Walk_Predicate );
		GameManager.Instance.InputMgr.BindCall( eInputCommands.MOVE_BACKWARD,	"BackwardEvent",	GoBackwardAction,		Motion_Walk_Predicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.MOVE_LEFT,		"LeftEvent",		StrafeLeftAction,		Motion_Walk_Predicate );
		GameManager.Instance.InputMgr.BindCall( eInputCommands.MOVE_RIGHT,		"RightEvent",		StrafeRightAction,		Motion_Walk_Predicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.STATE_RUN,		"RunEvent",			RunAction,				RunPredicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.STATE_JUMP,		"JumpEvent",		JumpAction,				JumpPredicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.USAGE,			"Interaction",		InteractionAction,		InteractionPredicate );
		GameManager.Instance.InputMgr.BindCall( eInputCommands.USAGE,			"Grab",				GrabAction,				GrabPredicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.GADGET3,			"Flashlight",		FlashlightAction,		FlashlightPredicate );

		GameManager.Instance.InputMgr.BindCall( eInputCommands.ABILITY_PRESS,	"DodgeStart",		AbilityEnableAction,	AbilityPredcate );
		GameManager.Instance.InputMgr.BindCall( eInputCommands.ABILITY_HOLD,	"DodgeContinue",	AbilityContinueAction,	AbilityPredcate );
		GameManager.Instance.InputMgr.BindCall( eInputCommands.ABILITY_RELEASE,	"DodgeEnd",			AbilityEndAction,		AbilityPredcate );
	}


	private	void	UnRegisterGroundedMotion()
	{
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.MOVE_FORWARD,	"ForwardEvent" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.MOVE_BACKWARD,	"BackwardEvent" );

		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.MOVE_LEFT,		"LeftEvent" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.MOVE_RIGHT,	"RightEvent" );
 
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.STATE_RUN,		"RunEvent" );

		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.STATE_JUMP,	"JumpEvent" );

		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.USAGE,			"Interaction" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.USAGE,			"Grab" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.GADGET3,		"Flashlight" );

		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.ABILITY_PRESS,	"DodgeStart" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.ABILITY_HOLD,	"DodgeContinue" );
		GameManager.Instance.InputMgr.UnbindCall( eInputCommands.ABILITY_RELEASE,"DodgeEnd" );
	}

	private	bool	Motion_Walk_Predicate()
	{
		return IsGrounded;
	}


	private	void	GoForwardAction()
	{
		m_States.IsMoving = true;

		float force = 1.0f;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;
		
		// Walking
		force *= ( bIsWalking ) ? m_WalkSpeed : 1.0f;
		
		// Crouch
		force *= ( m_States.IsCrouched )	? m_CrouchSpeed : 1.0f;

		// Running
		force *= ( m_States.IsRunning )		? m_RunSpeed : 1.0f;
		
		m_ForwardSmooth = force;
	}

	private	void	GoBackwardAction()
	{
		m_States.IsMoving = true;

		float force = 1.0f;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;
		
		// Walking
		force *= ( bIsWalking ) ? m_WalkSpeed : 1.0f;
		
		// Crouch
		force *= ( m_States.IsCrouched )	? m_CrouchSpeed : 1.0f;

		// Running
		force *= ( m_States.IsRunning )		? m_RunSpeed : 1.0f;
		
		m_ForwardSmooth = -force;
	}


	private	void	StrafeRightAction()
	{
		m_States.IsMoving = true;

		const float strafeFactor = 0.8f;
		float force = 1.0f;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;
		
		// Walking
		force *= ( bIsWalking ) ? m_WalkSpeed : 1.0f;
		
		// Crouch
		force *= ( m_States.IsCrouched )	? m_CrouchSpeed : 1.0f;

		// Running
		force *= ( m_States.IsRunning )		? m_RunSpeed : 1.0f;
		
		m_RightSmooth = force * strafeFactor;
	}


	private	void	StrafeLeftAction()
	{
		m_States.IsMoving = true;

		const float strafeFactor = 0.8f;
		float force = 1.0f;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;
		
		// Walking
		force *= ( bIsWalking ) ? m_WalkSpeed : 1.0f;
		
		// Crouch
		force *= ( m_States.IsCrouched )	? m_CrouchSpeed : 1.0f;

		// Running
		force *= ( m_States.IsRunning )		? m_RunSpeed : 1.0f;
		
		m_RightSmooth = -force * strafeFactor;
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

}
