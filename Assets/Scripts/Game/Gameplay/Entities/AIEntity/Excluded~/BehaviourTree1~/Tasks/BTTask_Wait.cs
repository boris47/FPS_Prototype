using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class BTTask_Wait : BTTaskNode
	{
		public override string NodeName => "Wait Action";
		public override string NodeInfo => "Wait for a certain amount of time";

		[Min(0f)]
		public float m_TimeToWait = 1f;

		[SerializeField, ReadOnly]
		protected float m_CurrentTimeToWait = 0f;

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTTask_Wait;
			node.m_TimeToWait = m_TimeToWait;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			m_CurrentTimeToWait = 0f;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool OnTryActivation()
		{
			m_CurrentTimeToWait = 0f;

			if (CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnFrame += OnFrame;
			}

			return true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
		{
			m_CurrentTimeToWait += InDeltaTime;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState PostUpdate()
		{
			EBTNodeState outState = EBTNodeState.RUNNING;
			if (m_CurrentTimeToWait > m_TimeToWait)
			{
				outState = EBTNodeState.SUCCEEDED;
			}
			return outState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in bool bIsAbort)
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}
		}
	}
}
