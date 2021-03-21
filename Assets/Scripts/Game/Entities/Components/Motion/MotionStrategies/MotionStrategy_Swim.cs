using UnityEngine;

public class MotionStrategy_Swim : MotionStrategyBase
{
	protected override MotionBindingsData[] m_Bindings => new MotionBindingsData[0];

	public	override	bool				CanMove						=> true;

	//////////////////////////////////////////////////////////////////////////
	protected override void Setup_Internal(Database.Section entitySection, params object[] args)
	{

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
	}
}
