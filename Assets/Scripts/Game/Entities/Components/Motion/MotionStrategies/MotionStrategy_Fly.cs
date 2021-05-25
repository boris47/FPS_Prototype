using UnityEngine;

public class MotionStrategy_Fly : MotionStrategyBase
{
	[SerializeField]
	private				float				m_WalkSpeed					= 0f;
	[SerializeField]
	private				float				m_RunSpeed					= 0f;

	protected override MotionBindingsData[] m_Bindings					=> new MotionBindingsData[]
	{
		new MotionBindingsData(EInputCommands.MOVE_FORWARD,			"ForwardEvent",     Action_MoveForward,     Predicate_Move),
		new MotionBindingsData(EInputCommands.MOVE_BACKWARD,		"BackwardEvent",    Action_MoveBackward,    Predicate_Move),
		new MotionBindingsData(EInputCommands.MOVE_LEFT,			"LeftEvent",        Action_MoveLeft,        Predicate_Move),
		new MotionBindingsData(EInputCommands.MOVE_RIGHT,			"RightEvent",       Action_MoveRight,       Predicate_Move),

		new MotionBindingsData(EInputCommands.STATE_RUN,			"RunEvent",         Action_Run,             Predicate_Run),
		new MotionBindingsData(EInputCommands.STATE_JUMP,			"JumpEvent",        Action_Jump,            Predicate_Jump),
		new MotionBindingsData(EInputCommands.STATE_CROUCH,			"CrouchEvent",		() => { },				null),
	};

	public	override	bool				CanMove						=> true;

	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection, params object[] args)
	{
		m_WalkSpeed		= entitySection.OfMultiValue<float>("Walk", 1);
		m_RunSpeed		= entitySection.OfMultiValue<float>("Run", 1);
	}

	public override void Enable()
	{
		base.Enable();

		// The entity is declared as flying
		m_States.IsCloseToGround = false;

		Rigidbody rigidBody = m_Entity.EntityRigidBody;
		rigidBody.drag = 7f;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void Move(EMovementType movementType, Vector3 direction)
	{
		float speed = 0f;
		m_Move = direction * speed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{
		base.OnFrame(deltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnLateFrame(float deltaTime)
	{
		base.OnLateFrame(deltaTime);

		if (!Utils.Math.SimilarZero(m_Move.magnitude))
		{
			Rigidbody rigidBody = m_Entity.EntityRigidBody;
			Vector3 translation = m_Entity.Head.TransformDirection(m_Move);
			rigidBody.velocity += translation;
			m_Move.LerpTo(Vector3.zero, deltaTime * 10f);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Move()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveForward()
	{
		m_States.IsMoving = true;

		bool bIsWalking = !m_States.IsRunning && !m_States.IsCrouched;

		//						Walking									Running		
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : 1.0f;

		m_Move.z = force;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveBackward()
	{
		m_States.IsMoving = true;

		bool bIsWalking = !m_States.IsRunning && !m_States.IsCrouched;

		//						Walking									Running		
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : 1.0f;

		m_Move.z = -force;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveRight()
	{
		m_States.IsMoving = true;

		bool bIsWalking = !m_States.IsRunning && !m_States.IsCrouched;

		//						Walking									Running		
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		m_Move.x = force * strafeFactor;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveLeft()
	{
		m_States.IsMoving = true;

		bool bIsWalking = !m_States.IsRunning && !m_States.IsCrouched;

		//						Walking									Running		
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		m_Move.x = -force * strafeFactor;
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Run()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Run()
	{
		m_States.IsRunning = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Jump()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Jump()
	{
		m_States.IsMoving = true;
		m_Move.y += m_States.IsRunning ? m_RunSpeed : m_WalkSpeed;
	}
}
