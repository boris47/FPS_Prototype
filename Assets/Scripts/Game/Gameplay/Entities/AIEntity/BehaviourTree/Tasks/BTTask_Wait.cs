using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Wait Action", "Wait for a certain amount of time")]
	public class BTTask_Wait : BTTaskNode
	{
		[Min(0f), ToNodeInspector(bShowLabel: true)]
		public		float		m_SecondsToWait			= 1f;

		//---------------------
		private		float		m_StartTime				= 0f;


		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTTask_Wait;
			node.m_SecondsToWait = m_SecondsToWait;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			m_StartTime = Time.time;
			return EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate()
		{
			return (m_StartTime + m_SecondsToWait < Time.time) ? EBTNodeState.SUCCEEDED : EBTNodeState.RUNNING;
		}
	}
}
