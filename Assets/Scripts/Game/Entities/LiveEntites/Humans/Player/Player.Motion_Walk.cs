
using UnityEngine;


public partial class Player {

	RigidbodyInterpolation prevInterpolation;

	//////////////////////////////////////////////////////////////////////////
	protected	override	void		EnterSimulationState()
	{
		this.m_MovementOverrideEnabled				= true;
		this.m_SimulationStartPosition				= this.transform.position;

//		GlobalManager.InputMgr.SetCategory(InputCategory.CAMERA, false);
//		InputManager.IsEnabled					= false;
		GlobalManager.InputMgr.DisableCategory( EInputCategory.ALL );

		this.prevInterpolation						= this.m_RigidBody.interpolation;
		this.m_RigidBody.interpolation				= RigidbodyInterpolation.Interpolate;

		this.DropEntityDragged();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override		void		BeforeSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		CameraControl.Instance.Target = target;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool		SimulateMovement( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		// END OF SIMULATION STEP
		Vector3 direction = ( destination - this.m_SimulationStartPosition );
		float simulationdDistanceToTravel = direction.sqrMagnitude;
		float simulationDistanceTravelled = (this.transform.position - this.m_SimulationStartPosition ).sqrMagnitude;
		
		if ( simulationDistanceTravelled > simulationdDistanceToTravel )
		{
			this.m_SimulationStartPosition = this.transform.position;
			return false;				// force logic update
		}

		float interpolant = simulationDistanceTravelled / simulationdDistanceToTravel;

		// TIME SCALE
		float timeScale = Mathf.Lerp( Time.timeScale, timeScaleTarget, interpolant );
		Time.timeScale = Mathf.Clamp01( timeScale );
		SoundManager.Pitch = timeScale;
		
		CameraControl.Instance.Target = target;

		//	POSITION BY DISTANCE
		{
			bool isWalking	= ( movementType != ESimMovementType.RUN );
			bool isRunning	= !isWalking;
			bool isCrouched = ( movementType == ESimMovementType.CROUCHED );
			this.m_ForwardSmooth = ( movementType != ESimMovementType.STATIONARY ) ? ( isCrouched ) ? this.m_CrouchSpeed : ( isRunning ) ? this.m_RunSpeed : this.m_WalkSpeed : 0.0f;

			this.m_States.IsWalking	= isWalking;
			this.m_States.IsRunning	= isRunning;
			this.m_States.IsCrouched	= isCrouched;
			this.m_States.IsMoving	= movementType != ESimMovementType.STATIONARY;

			this.m_Move = (this.m_ForwardSmooth * direction.normalized ) * this.GroundSpeedModifier;
//			m_RigidBody.velocity = m_Move;
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override		void		AfterSimulationStage( ESimMovementType movementType, Vector3 destination, Transform target, float timeScaleTarget )
	{
		CameraControl.Instance.Target = null;
		this.m_Move = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void		ExitSimulationState()
	{
		this.m_MovementOverrideEnabled = false;
		this.m_SimulationStartPosition = Vector3.zero;

		this.m_RigidBody.interpolation = this.prevInterpolation;

		if ( CameraControl.Instance.Target != null )
		{
			Vector3 projectedTarget = Utils.Math.ProjectPointOnPlane(this.transform.up, this.transform.position, CameraControl.Instance.Target.position );
			Quaternion rotation = Quaternion.LookRotation( projectedTarget - this.transform.position, this.transform.up );
			this.transform.rotation = rotation;
		}

		this.m_Move					= Vector3.zero;
		this.m_ForwardSmooth			= 0f;
		this.m_States.IsWalking		= false;
		this.m_States.IsRunning		= false;
		this.m_States.IsMoving		= false;
		Time.timeScale			= 1f;

//		GlobalManager.InputMgr.SetCategory(InputCategory.CAMERA, true);
//		InputManager.IsEnabled = true;
		GlobalManager.InputMgr.EnableCategory( EInputCategory.ALL );
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
		GlobalManager.InputMgr.BindCall( EInputCommands.MOVE_FORWARD,		"ForwardEvent", this.GoForwardAction, this.Motion_Walk_Predicate );
		GlobalManager.InputMgr.BindCall( EInputCommands.MOVE_BACKWARD,		"BackwardEvent", this.GoBackwardAction, this.Motion_Walk_Predicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.MOVE_LEFT,			"LeftEvent", this.StrafeLeftAction, this.Motion_Walk_Predicate );
		GlobalManager.InputMgr.BindCall( EInputCommands.MOVE_RIGHT,			"RightEvent", this.StrafeRightAction, this.Motion_Walk_Predicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.STATE_RUN,			"RunEvent", this.RunAction, this.RunPredicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.STATE_JUMP,			"JumpEvent", this.JumpAction, this.JumpPredicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.USAGE,				"Interaction", this.InteractionAction, this.InteractionPredicate );
		GlobalManager.InputMgr.BindCall( EInputCommands.USAGE,				"Grab", this.GrabAction, this.GrabPredicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.GADGET3,			"Flashlight", this.FlashlightAction, this.FlashlightPredicate );

		GlobalManager.InputMgr.BindCall( EInputCommands.ABILITY_PRESS,		"DodgeStart", this.AbilityEnableAction, this.AbilityPredcate );
		GlobalManager.InputMgr.BindCall( EInputCommands.ABILITY_HOLD,		"DodgeContinue", this.AbilityContinueAction, this.AbilityPredcate );
		GlobalManager.InputMgr.BindCall( EInputCommands.ABILITY_RELEASE,	"DodgeEnd", this.AbilityEndAction, this.AbilityPredcate );
	}
	

	//////////////////////////////////////////////////////////////////////////
	private	void	UnRegisterGroundedMotion()
	{
		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_FORWARD,		"ForwardEvent" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_BACKWARD,	"BackwardEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_LEFT,		"LeftEvent" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.MOVE_RIGHT,		"RightEvent" );
 
		GlobalManager.InputMgr.UnbindCall( EInputCommands.STATE_RUN,		"RunEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.STATE_JUMP,		"JumpEvent" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.USAGE,			"Interaction" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.USAGE,			"Grab" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.GADGET3,			"Flashlight" );

		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_PRESS,	"DodgeStart" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_HOLD,		"DodgeContinue" );
		GlobalManager.InputMgr.UnbindCall( EInputCommands.ABILITY_RELEASE,	"DodgeEnd" );
	}


	//////////////////////////////////////////////////////////////////////////
	private	bool	Motion_Walk_Predicate()
	{
		return this.IsGrounded;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	GoForwardAction()
	{
		this.m_States.IsMoving = true;

		bool bIsWalking = this.m_States.IsRunning == false && this.m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = ( bIsWalking ) ? this.m_WalkSpeed : (this.m_States.IsRunning ) ? this.m_RunSpeed : (this.m_States.IsCrouched )	? this.m_CrouchSpeed : 1.0f;

		this.m_ForwardSmooth = force;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	GoBackwardAction()
	{
		this.m_States.IsMoving = true;

		bool bIsWalking = this.m_States.IsRunning == false && this.m_States.IsCrouched == false;
		
		//						Walking									Running									Crouch
		float force = ( bIsWalking ) ? this.m_WalkSpeed : (this.m_States.IsRunning ) ? this.m_RunSpeed : (this.m_States.IsCrouched ) ? this.m_CrouchSpeed : 1.0f;

		this.m_ForwardSmooth = -force;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	StrafeRightAction()
	{
		this.m_States.IsMoving = true;

		bool bIsWalking = this.m_States.IsRunning == false && this.m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = ( bIsWalking ) ? this.m_WalkSpeed : (this.m_States.IsRunning ) ? this.m_RunSpeed : (this.m_States.IsCrouched ) ? this.m_CrouchSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		this.m_RightSmooth = force * strafeFactor;
	}
	

	//////////////////////////////////////////////////////////////////////////
	private	void	StrafeLeftAction()
	{
		this.m_States.IsMoving = true;

		bool bIsWalking = this.m_States.IsRunning == false && this.m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = ( bIsWalking ) ? this.m_WalkSpeed : (this.m_States.IsRunning ) ? this.m_RunSpeed : (this.m_States.IsCrouched ) ? this.m_CrouchSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		this.m_RightSmooth = -force * strafeFactor;
	}


	//////////////////////////////////////////////////////////////////////////
	private	bool	RunPredicate()
	{
		return true;//( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	RunAction()
	{
		if (this.m_States.IsMoving )
		{
			this.m_States.IsCrouched = false;
			this.m_States.IsRunning = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	bool	JumpPredicate()
	{
		return this.IsGrounded && this.m_States.IsJumping == false && this.m_States.IsHanging == false && this.m_States.IsFalling == false && this.m_GrabbedObject == null;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	JumpAction()
	{
		this.m_UpSmooth = this.m_JumpForce / (this.m_States.IsCrouched ? 1.5f : 1.0f );
		this.m_States.IsJumping = true;
	}

}
