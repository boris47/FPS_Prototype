using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Wait Action", "Wait for a certain amount of time")]
	public class BTTask_Wait : BTTaskNode
	{
		protected class RuntimeData : RuntimeDataBase
		{
			public	float	StartTime = 0f;
		}

		[Min(0f), ToNodeInspector(InValueWidth: 50f)]
		public		float		m_SecondsToWait			= 1f;

		//---------------------

		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			nodeRuntimeData.StartTime = Time.time;

			return EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			RuntimeData nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

			return (nodeRuntimeData.StartTime + m_SecondsToWait < Time.time) ? EBTNodeState.SUCCEEDED : EBTNodeState.RUNNING;
		}
	}
}
