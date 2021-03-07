using System.Collections.Generic;
using UnityEngine;

public class MotionStrategy_Platform : MotionStrategy_Walk
{
//	[SerializeField]
//	private		float				m_VerticalSpeed				= 0f;

//	[SerializeField]
//	private		float				m_Scale					= 0f;
	
	//////////////////////////////////////////////////////////////////////////
	public override void Enable()
	{
		base.Enable();

	//	UnityEngine.Assertions.Assert.IsNotNull(m_Entity.transform.parent);
	}
	
	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
	 	base.OnPhysicFrame(fixedDeltaTime);
		/*
		Rigidbody rigidBody = m_Entity.AsInterface.RigidBody;

	//	float drag = m_Entity.IsGrounded ? 7f : 0.0f;
		rigidBody.drag = 10f;
		*/
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{
		base.OnFrame(deltaTime);

	//	if (!m_Entity.IsGrounded)
	//	{
	//		m_VerticalSpeed -= Mathf.Abs(Physics.gravity.y) * deltaTime * 0.06f;
	//
	//		m_Entity.transform.Translate(0f, m_VerticalSpeed, 0f, Space.Self);
	//	}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnLateFrame(float deltaTime)
	{
	//	Rigidbody rigidBody = m_Entity.AsInterface.RigidBody;

	//	if (m_bInputOverride)
	//	{
	//		rigidBody.velocity = m_Move;
	//		return;
	//	}
	//	else
	//	{
	//		// intercept value and capture it
	//		if (m_Move.y > 0.0f)
	//		{
	//			m_VerticalSpeed = m_Move.y * 0.02f;
	//			m_Move.y = 0f;
	//			m_Entity.transform.Translate(0f, m_VerticalSpeed, 0f, Space.Self);
	//		}
	//	}

		base.OnLateFrame(deltaTime);
	}

}
