using System.Collections.Generic;
using UnityEngine;

public class MotionStrategy_Empty : MotionStrategyBase
{
	protected override List<MotionBindingsData> m_Bindings => new List<MotionBindingsData>();

	//////////////////////////////////////////////////////////////////////////
	public override void OverrideMove(ESimMovementType movementType, Vector3 direction)
	{
		m_Move = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		Rigidbody rigidBody = m_Entity.AsInterface.RigidBody;

		rigidBody.velocity = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{
		
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnLateFrame(float deltaTime)
	{
		
	}
}
