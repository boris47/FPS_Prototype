using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class BTTask_Wait : BTTaskNode
	{
		public override string NodeName => "Wait Action";
		public override string NodeInfo => "Wait for a certain amount of time";

		[Min(0f), ToNodeInspector(bShowLabel: true)]
		public		float		m_TimeToWait			= 1f;

		[SerializeField, ReadOnly]
		private		float		m_CurrentTimeToWait		= 0f;


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
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			m_CurrentTimeToWait = 0f;

			if (CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnFrame += OnFrame;
			}
			else
			{
				OutState = EBTNodeState.FAILED;
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate()
		{
			return m_CurrentTimeToWait >= m_TimeToWait ? EBTNodeState.SUCCEEDED : EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnFrame(float InDeltaTime)
		{
			m_CurrentTimeToWait += InDeltaTime;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate()
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnFrame -= OnFrame;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{
			m_CurrentTimeToWait = 0f;
		}
	}
}
