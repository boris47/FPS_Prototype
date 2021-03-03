using System.Collections.Generic;
using UnityEngine;

public class MotionStrategy_Walk : MotionStrategyBase
{
	protected override List<MotionBindingsData> m_Bindings => new List<MotionBindingsData>()
	{
		new MotionBindingsData( EInputCommands.MOVE_FORWARD,		"ForwardEvent",		Action_MoveForward,		Predicate_Move ),
		new MotionBindingsData( EInputCommands.MOVE_BACKWARD,		"BackwardEvent",	Action_MoveBackward,	Predicate_Move ),
		
		new MotionBindingsData( EInputCommands.MOVE_LEFT,			"LeftEvent",		Action_MoveLeft,		Predicate_Move ),
		new MotionBindingsData( EInputCommands.MOVE_RIGHT,			"RightEvent",		Action_MoveRight,		Predicate_Move ),
		
		new MotionBindingsData( EInputCommands.STATE_RUN,			"RunEvent",			Action_Run,				Predicate_Run ),
		
		new MotionBindingsData( EInputCommands.STATE_JUMP,			"JumpEvent",		Action_Jump,			Predicate_Jump ),
	};

	[SerializeField]
	private		float				m_WalkSpeed					= 0f;
	[SerializeField]
	private		float				m_RunSpeed					= 0f;
	[SerializeField]
	private		float				m_CrouchSpeed				= 0f;
	[SerializeField]
	private		float				m_ClimbSpeed				= 0f;
	[SerializeField]
	private		float				m_JumpForce					= 0f;


	//////////////////////////////////////////////////////////////////////////
	public override void Setup(Entity entity, Transform head, Transform body)
	{
		base.Setup(entity, head, body);

		string sectionName = entity.AsInterface.Section;

		UnityEngine.Assertions.Assert.IsTrue(GlobalManager.Configs.TryGetSection(sectionName, out Database.Section section));

		m_WalkSpeed		= section.OfMultiValue("Walk", 1, 0f);
		m_RunSpeed		= section.OfMultiValue("Run", 1, 0f);
		m_CrouchSpeed	= section.OfMultiValue("Crouch", 1, 0f);
		m_ClimbSpeed	= section.AsFloat("Climb");
		m_JumpForce		= section.OfMultiValue("Jump", 1, 0f);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OverrideMove(ESimMovementType movementType, Vector3 direction)
	{
		float speed = (movementType != ESimMovementType.STATIONARY) ? (m_States.IsCrouched) ? m_CrouchSpeed : (m_States.IsRunning) ? m_RunSpeed : m_WalkSpeed : 0.0f;
		m_Move = direction * speed;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		base.OnPhysicFrame(fixedDeltaTime);

		Rigidbody rigidBody = m_Entity.AsInterface.RigidBody;

		rigidBody.angularVelocity = Vector3.zero;

		float drag = m_Entity.IsGrounded ? 7f : 0.0f;
		rigidBody.drag = drag;

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

		Rigidbody rigidBody = m_Entity.AsInterface.RigidBody;

		if (m_bInputOverride)
		{
			rigidBody.velocity = m_Move;
			return;
		}
		else
		{
			if (m_Entity.IsGrounded)
			{
				// Forward and strafe
				Vector3 localVelocity = rigidBody.transform.InverseTransformDirection(rigidBody.velocity);
				{
					localVelocity.z = m_Move.z;
					localVelocity.x = m_Move.x;
				}
				rigidBody.velocity = rigidBody.transform.TransformDirection(localVelocity);

				// Jump
				if (m_Move.y > 0.0f)
				{
					Vector3 up = m_Body.up;
					rigidBody.AddForce(up * m_Move.y, ForceMode.VelocityChange);
				}

				m_Move.Set(0f, 0f, 0f);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected bool Predicate_Move()
	{
		return m_Entity.IsGrounded;
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
		return true;//( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched ); // TODO re-implement
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
		return m_Entity.IsGrounded && !m_States.IsJumping && !m_States.IsHanging && !m_States.IsFalling;// && m_CurrentGrabbed == null;
	}


	//////////////////////////////////////////////////////////////////////////
	protected void Action_Jump()
	{
		m_Move.y = m_JumpForce / (m_States.IsCrouched ? 1.5f : 1.0f);
		m_States.IsJumping = true;
	}
}
