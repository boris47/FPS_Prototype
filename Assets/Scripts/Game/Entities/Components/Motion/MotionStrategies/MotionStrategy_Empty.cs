using UnityEngine;

public class MotionStrategy_Empty : MotionStrategyBase
{
	protected override MotionBindingsData[] m_Bindings => new MotionBindingsData[0];

	public	override	bool				CanMove						=> true;


	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection)
	{
		
	}

	//////////////////////////////////////////////////////////////////////////
	public override void Move(EMovementType movementType, Vector3 direction)
	{
		m_Move.Set(0f, 0f, 0f);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnPhysicFrame(float fixedDeltaTime)
	{
		Rigidbody rigidBody = m_Entity.EntityRigidBody;

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
