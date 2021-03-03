﻿using System.Collections.Generic;
using UnityEngine;

public class MotionStrategy_Swim : MotionStrategyBase
{
	protected override List<MotionBindingsData> m_Bindings => new List<MotionBindingsData>();


	//////////////////////////////////////////////////////////////////////////
	public override void OverrideMove(ESimMovementType movementType, Vector3 direction)
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
