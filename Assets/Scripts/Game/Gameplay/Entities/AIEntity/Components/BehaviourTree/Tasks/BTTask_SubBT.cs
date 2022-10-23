using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Sub Behaviour Tree", "Execute a sub behaviour tree")]
	public sealed class BTTask_SubBT : BTTaskNode
	{
		private class RuntimeData : RuntimeDataBase
		{
			[ReadOnly]
			public BehaviourTreeInstanceData SubTreeInstanceData = null;
		}

		[SerializeField, ToNodeInspector(bInShowDefaultLabel: true)]
		public BehaviourTree			m_SubBehaviourTreeAsset				= null;


		//////////////////////////////////////////////////////////////////////////
		protected override RuntimeDataBase CreateRuntimeDataInstance(in BTNodeInstanceData InThisNodeInstanceData) => new RuntimeData();

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BTNodeInstanceData InThisNodeInstanceData)
		{
			base.OnAwakeInternal(InThisNodeInstanceData);

			if (m_SubBehaviourTreeAsset.IsNotNull())
			{
				var nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);

				AIController controller = InThisNodeInstanceData.BehaviourTreeInstanceData.Controller;
				nodeRuntimeData.SubTreeInstanceData = BehaviourTree.CreateInstanceFrom(m_SubBehaviourTreeAsset, controller);

				m_SubBehaviourTreeAsset.OnAwake(nodeRuntimeData.SubTreeInstanceData);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			if (m_SubBehaviourTreeAsset.IsNotNull())
			{
				var nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
				m_SubBehaviourTreeAsset.StartTree(nodeRuntimeData.SubTreeInstanceData);
			}
			else
			{
				OutState = EBTNodeState.FAILED;
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in BTNodeInstanceData InThisNodeInstanceData, in float InDeltaTime)
		{
			var nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData);
			return m_SubBehaviourTreeAsset.UpdateTree(nodeRuntimeData.SubTreeInstanceData, InDeltaTime);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeAbort(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_SubBehaviourTreeAsset.IsNotNull())
			{
				var nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData); 
				m_SubBehaviourTreeAsset.StopTree(nodeRuntimeData.SubTreeInstanceData);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_SubBehaviourTreeAsset.IsNotNull())
			{
				var nodeRuntimeData = GetRuntimeData<RuntimeData>(InThisNodeInstanceData); 
				m_SubBehaviourTreeAsset.ResetTree(nodeRuntimeData.SubTreeInstanceData);
			}
		}
	}
}
