using UnityEngine;

public class MotionStrategy_Climb : MotionStrategyBase
{
	[SerializeField]
	private				float				m_ClimbSpeed				= 0f;
	[SerializeField]
	private				float				m_JumpForce					= 0f;

	private				bool				m_MakeJump					= false;

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

	private ClimbableObject m_ClimbableVolume = null;

	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection, params object[] args)
	{
		m_ClimbSpeed	= entitySection.AsFloat("Climb");
		m_JumpForce		= entitySection.OfMultiValue<float>("Jump", 1);

		if (args.IsValidIndex(0))
		{
			if (args[0] is ClimbableObject climbableVolume)
			{
				m_ClimbableVolume = climbableVolume;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Move(EMovementType movementType, Vector3 direction)
	{
		float speed = m_ClimbSpeed;
		m_Move = direction * speed;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Enable()
	{
		base.Enable();

		// Becasue we are using this strategy could be 
		m_States.IsCloseToGround = false;

		m_Entity.PhysicCollider.material = new PhysicMaterial()
		{
			dynamicFriction = 0f,
			staticFriction = 0f,
		};
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Disable()
	{
		base.Disable();

		Object.Destroy(m_Entity.PhysicCollider.material);

		m_Entity.PhysicCollider.material = null;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);

	//	Rigidbody rigidBody = m_Entity.EntityRigidBody;

	//	rigidBody.angularVelocity = Vector3.zero;

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
		Transform head = m_Entity.Head;
		
		Vector3 localHeadDirection = rigidBody.transform.InverseTransformDirection(head.forward);
		if (m_MakeJump)
		{
			rigidBody.drag = 0f;

			// The entity is declared as flying
			m_States.IsCloseToGround = false;

			rigidBody.velocity = (head.forward + (head.right * Mathf.Sign(m_Move.x))).normalized * m_JumpForce;

			m_States.IsAcending = localHeadDirection.y > 0.01f;
			m_States.IsDescending = localHeadDirection.y < -0.01f;
			m_Entity.Motion.SetMotionType(EEntityMotionType.GROUNDED);
		}
		else
		{
			Vector3 localVelocity = rigidBody.transform.InverseTransformDirection(rigidBody.velocity);

			m_States.IsAcending = localVelocity.y > 0.01f;
			m_States.IsDescending = localVelocity.y < -0.01f;

			if (!Utils.Math.SimilarZero(m_Move.magnitude))
			{
				// Up and Down
				{
					localVelocity.x = m_Move.x;
					localVelocity.y = m_Move.y * Mathf.Sign(localHeadDirection.y);
					localVelocity.z = m_Move.z;
				}
				rigidBody.velocity = rigidBody.transform.TransformDirection(localVelocity);
		

				m_Move.LerpTo(Vector3.zero, deltaTime * 10f);
			}
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

		float speed = m_ClimbSpeed * (m_States.IsRunning ? 1.5f : 1f);

		// Entity facing the Climbable
		if (Vector3.Dot(m_Head.forward, m_ClimbableVolume.transform.forward) >= 0f)
		{
			m_Move.z = speed * 0.2f;
			m_Move.y = speed;
		}
		else
		{
			m_Move.z = speed;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveBackward()
	{
		m_States.IsMoving = true;

		float speed = m_ClimbSpeed * (m_States.IsRunning ? 1.5f : 1f);

		// Entity facing the Climbable
		if (Vector3.Dot(m_Head.forward, m_ClimbableVolume.transform.forward) < 0.8f)
		{
			m_Move.z = -speed * 0.2f;
			m_Move.y = -speed;
		}
		else
		{
			m_Move.z = -speed;
		}

	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveRight()
	{
		m_Move.x = 1f;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveLeft()
	{
		m_Move.x = -1f;
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
	//	m_Move.z = m_JumpForce;
	//	m_States.IsJumping = true;

		m_MakeJump = true;
	}
}
