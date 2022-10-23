
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public class BehaviourTreeInstanceData
	{
		private static uint s_Id = 0u;
		private readonly uint m_UniqueId = s_Id++;

		[SerializeField, ReadOnly]
		private				BehaviourTree					m_TreeAsset									= null;
		[SerializeField, ReadOnly]
		private				BehaviourTree					m_TreeInstance								= null;
		[SerializeField, ReadOnly]
		private				AIController					m_Controller								= null;
		[SerializeField, ReadOnly]
		private				BlackboardInstanceData			m_BlackboardInstanceData					= null;
		[SerializeField, ReadOnly]
		private				BTNodeInstanceData[]			m_NodesInstanceData							= null;
		[SerializeReference, ReadOnly]
		private				RuntimeDataBase[]				m_NodesRuntimeData							= null;

		[SerializeField, ReadOnly]
		private				BTNodeInstanceData				m_RootNode									= null;
		[SerializeField, ReadOnly]
		private				BTNodeInstanceData				m_CurrentRunningNode						= null;
		[SerializeField, ReadOnly]
		private				bool							m_RunningNodeLocked							= false;
		[SerializeField, ReadOnly]
		private				EBehaviourTreeState				m_TreeState									= EBehaviourTreeState.INVALID;


		public				uint							UniqueId									=> m_UniqueId;
		public				BehaviourTree					TreeAsset									=> m_TreeAsset;
		public				BehaviourTree					TreeInstance								=> m_TreeInstance;
		public				AIController					Controller									=> m_Controller;
		public				BlackboardInstanceData			Blackboard									=> m_BlackboardInstanceData;
		public				BTNodeInstanceData[]			NodesInstanceData							=> m_NodesInstanceData;
		
		public				BTNodeInstanceData				RootNode									=> m_RootNode;
		public				BTNodeInstanceData				CurrentRunningNode							=> m_CurrentRunningNode;
		public				bool							IsRunningNodeLocked							=> m_RunningNodeLocked;
		public				EBehaviourTreeState				TreeState									=> m_TreeState;



		//////////////////////////////////////////////////////////////////////////
		public BehaviourTreeInstanceData(in BehaviourTree InBehaviourTreeAsset, in BehaviourTree InTreeInstance, in AIController InController)
		{
			m_TreeAsset = InBehaviourTreeAsset;
			m_TreeInstance = InTreeInstance;
			m_Controller = InController;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetBlackboardInstance(in BlackboardInstanceData InBlackboardInstanceData)
		{
			m_BlackboardInstanceData = InBlackboardInstanceData;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetNodesInstancesData(in BTNodeInstanceData[] InNodesInstanceData)
		{
			m_NodesInstanceData = InNodesInstanceData;
			
			m_RootNode = InNodesInstanceData[0];
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetNodesRuntimeData(in RuntimeDataBase[] InNodesRuntimeData)
		{
			m_NodesRuntimeData = InNodesRuntimeData;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetTreeState(in EBehaviourTreeState InNewState)
		{
			m_TreeState = InNewState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNodeLocked(in bool bLocked)
		{
			m_RunningNodeLocked = bLocked;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNode(in BTNodeInstanceData InNode)
		{
			m_CurrentRunningNode = InNode;
		}

		//////////////////////////////////////////////////////////////////////////
		public T GetRuntimeData<T>(uint InNodeIndex) where T : RuntimeDataBase
		{
			return m_NodesRuntimeData.At(InNodeIndex) as T;
		}
	}
}