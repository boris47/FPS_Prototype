using UnityEngine;

public class MotionStrategy_Climb : MotionStrategyBase
{
	[SerializeField]
	private				float				m_ClimbSpeed				= 0f;
	[SerializeField]
	private				float				m_JumpForce					= 0f;

	protected override MotionBindingsData[] m_Bindings					=> new MotionBindingsData[]
	{
		new MotionBindingsData( EInputCommands.MOVE_FORWARD,        "ForwardEvent",     Action_MoveForward,     Predicate_Move  ),
		new MotionBindingsData( EInputCommands.MOVE_BACKWARD,       "BackwardEvent",    Action_MoveBackward,    Predicate_Move  ),
		new MotionBindingsData( EInputCommands.MOVE_LEFT,           "LeftEvent",        Action_MoveLeft,        Predicate_Move  ),
		new MotionBindingsData( EInputCommands.MOVE_RIGHT,          "RightEvent",       Action_MoveRight,       Predicate_Move  ),

		new MotionBindingsData( EInputCommands.STATE_RUN,           "RunEvent",         Action_Run,             Predicate_Run   ),
		new MotionBindingsData( EInputCommands.STATE_JUMP,          "JumpEvent",        Action_Jump,            Predicate_Jump  ),
		new MotionBindingsData( EInputCommands.STATE_CROUCH,		"CrouchEvent",		() => { },				null            ),
	};

	public	override	bool				CanMove						=> true;

	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection)
	{
		m_ClimbSpeed	= entitySection.AsFloat("Climb");
		m_JumpForce		= entitySection.OfMultiValue<float>("Jump", 1);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Move(EMovementType movementType, Vector3 direction)
	{
		float speed = m_ClimbSpeed;
		m_Move = direction * speed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);

		Rigidbody rigidBody = m_Entity.EntityRigidBody;

		rigidBody.angularVelocity = Vector3.zero;

	//	float drag = m_Entity.IsGrounded ? 7f : 0.0f;
	//	rigidBody.drag = drag;

		// add RELATIVE gravity force
	//	Vector3 gravity = m_Body.up * Physics.gravity.y;
	//	rigidBody.AddForce(gravity, ForceMode.Acceleration);
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

		Rigidbody rigidBody = m_Entity.EntityRigidBody;
		
		// Up and Down
		Vector3 localVelocity = rigidBody.transform.InverseTransformDirection(rigidBody.velocity);
		{
			localVelocity.y = m_Move.y;
		}
		rigidBody.velocity = rigidBody.transform.TransformDirection(localVelocity);

		m_States.IsAcending = m_Move.y > 0f;
		m_States.IsDescending = m_Move.y < 0f;

		m_Move.Set(0f, 0f, 0f);
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

		float speed = m_ClimbSpeed * (m_States.IsRunning ? 1.5f : 1f);

		m_Move.y = speed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveBackward()
	{
		m_States.IsMoving = true;

		float speed = m_ClimbSpeed * (m_States.IsRunning ? 1.5f : 1f);

		m_Move.y = -speed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveRight()
	{
		// TODO directional jump
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveLeft()
	{
		// TODO directional jump
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Run()
	{
		return true;//( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched ); // TODO re-implement
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Run()
	{
		m_States.IsRunning = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Jump()
	{
		return !m_States.IsJumping && !m_States.IsAcending && !m_States.IsDescending && !m_Entity.Interactions.IsHoldingObject;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Jump()
	{
		// TODO Jump in direction
	//	m_Move.y = m_JumpForce;
	//	m_States.IsJumping = true;
	}
}
