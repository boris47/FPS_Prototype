using UnityEngine;

public class MotionStrategy_Grounded : MotionStrategyBase
{
	[SerializeField]
	private				float				m_WalkSpeed					= 0f;
	[SerializeField]
	private				float				m_RunSpeed					= 0f;
	[SerializeField]
	private				float				m_CrouchSpeed				= 0f;
	[SerializeField]
	private				float				m_ClimbSpeed				= 0f;
	[SerializeField]
	private				float				m_JumpForce					= 0f;
	[SerializeField]
	private				Foots				m_Foots						= null;

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

	public override		bool				CanMove						=> m_States.IsCloseToGround;

	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection, params object[] args)
	{
		if (!m_Foots)
		{
			Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL_AND_CHILDREN, out m_Foots);
		}

		// This prevent foot from having collision or trigger with other entity collider
		m_Entity.SetCollisionStateWith(m_Foots.Collider, false);

		m_WalkSpeed		= entitySection.OfMultiValue<float>("Walk", 1);
		m_RunSpeed		= entitySection.OfMultiValue<float>("Run", 1);
		m_CrouchSpeed	= entitySection.OfMultiValue<float>("Crouch", 1);
		m_ClimbSpeed	= entitySection.AsFloat("Climb");
		m_JumpForce		= entitySection.OfMultiValue<float>("Jump", 1);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Enable()
	{
		base.Enable();
		
		if (CustomAssertions.IsNotNull(m_Foots))
		{
			m_Foots.OnEvent_GroundedChanged += OnGroundedChange;
		}

		// Entity is enable as fly, so we ensure rigidbody drag is Zero
		if (!m_States.IsCloseToGround)
		{
			m_Entity.EntityRigidBody.drag = 0;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Disable()
	{
		if (CustomAssertions.IsNotNull(m_Foots))
		{
			m_Foots.OnEvent_GroundedChanged -= OnGroundedChange;
		}

		base.Disable();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Move(EMovementType movementType, Vector3 direction)
	{
		float speed = (movementType != EMovementType.STATIONARY) ? (m_States.IsCrouched) ? m_CrouchSpeed : (m_States.IsRunning) ? m_RunSpeed : m_WalkSpeed : 0.0f;
		m_Move = direction * speed;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnGroundedChange(bool newState)
	{
		m_States.IsCloseToGround = newState;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);

		Rigidbody rigidBody = m_Entity.EntityRigidBody;

	//	rigidBody.angularVelocity = Vector3.zero;
		rigidBody.drag = m_States.IsCloseToGround ? 7f : 0.0f;

		// add RELATIVE gravity force
		Vector3 gravity = m_Body.up * Physics.gravity.y;
		rigidBody.AddForce(gravity, ForceMode.Acceleration);
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
			Vector3 localVelocity = rigidBody.transform.InverseTransformDirection(rigidBody.velocity);
			if (m_States.IsCloseToGround)
			{
				// Forward
				localVelocity.z = m_Move.z;
				// Right
				localVelocity.x = m_Move.x;
				rigidBody.velocity = rigidBody.transform.TransformDirection(localVelocity);

				// Jump
				if (m_Move.y > 0.0f)
				{
					Vector3 up = m_Body.up;
					rigidBody.AddForce(up * m_Move.y, ForceMode.VelocityChange);
					m_Move.y = 0f;
				}
			}
			else
			{
				// Forward
				localVelocity.z += m_Move.z * deltaTime;
				// Right
				localVelocity.x += m_Move.x * deltaTime;
				rigidBody.velocity = rigidBody.transform.TransformDirection(localVelocity);

				m_States.IsAcending = localVelocity.y > 0.01f;
				m_States.IsDescending = localVelocity.y < -0.01f;
			}

			m_Move.LerpTo(Vector3.zero, deltaTime * 10f);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Move()
	{
		return true;// m_States.IsCloseToGround;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveForward()
	{
		m_States.IsMoving = true;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : (m_States.IsCrouched) ? m_CrouchSpeed : 1.0f;

		m_Move.z = force;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveBackward()
	{
		m_States.IsMoving = true;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : (m_States.IsCrouched) ? m_CrouchSpeed : 1.0f;

		m_Move.z = -force;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveRight()
	{
		m_States.IsMoving = true;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : (m_States.IsCrouched) ? m_CrouchSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		m_Move.x = force * strafeFactor;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_MoveLeft()
	{
		m_States.IsMoving = true;

		bool bIsWalking = m_States.IsRunning == false && m_States.IsCrouched == false;

		//						Walking									Running									Crouch
		float force = (bIsWalking) ? m_WalkSpeed : (m_States.IsRunning) ? m_RunSpeed : (m_States.IsCrouched) ? m_CrouchSpeed : 1.0f;

		const float strafeFactor = 0.8f;
		m_Move.x = -force * strafeFactor;
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Run()
	{
		return m_States.IsCloseToGround;// true;//( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched ); // TODO re-implement
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Run()
	{
		m_States.IsCrouched = false;
		m_States.IsRunning = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Jump()
	{
		return m_States.IsCloseToGround && !m_States.IsJumping && !m_States.IsAcending && !m_States.IsDescending && !m_Entity.Interactions.IsHoldingObject;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Jump()
	{
		m_Move.y = m_JumpForce / (m_States.IsCrouched ? 1.5f : 1.0f);
		m_States.IsJumping = true;
	}
}
