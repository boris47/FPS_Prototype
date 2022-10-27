
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class NodeAbortCandidateRequest
	{
		private readonly BTNodeInstanceData m_Candidate = null;
		private readonly System.Action<BTNodeInstanceData> m_OnRequestAccepted = delegate { };

		public BTNodeInstanceData Candidate => m_Candidate;

		public NodeAbortCandidateRequest(in BTNodeInstanceData InCandidate, in System.Action<BTNodeInstanceData> InOnRequestAccepted)
		{
			m_Candidate = InCandidate;
			m_OnRequestAccepted = InOnRequestAccepted;
		}

		public void AcceptRequest() => m_OnRequestAccepted(m_Candidate);
	}

	public class TickableEntry
	{
		private readonly BTNodeInstanceData m_NodeInstanceData = null;
		private readonly System.Action<BTNodeInstanceData, float> m_OnTick = delegate { };

		public BTNodeInstanceData NodeInstanceData => m_NodeInstanceData;

		public TickableEntry(in BTNodeInstanceData InNodeInstanceData, in System.Action<BTNodeInstanceData, float> InOnTick)
		{
			m_NodeInstanceData = InNodeInstanceData;
			m_OnTick = InOnTick;
		}

		public void Tick(in float InDeltaTime) => m_OnTick(m_NodeInstanceData, InDeltaTime);
	}

	[System.Serializable]
	public class BehaviourTreeInstanceData
	{
		private static		uint							 s_Id										= 0u;

		//--------------------- Static
		private readonly	uint							m_UniqueId									= s_Id++;
	//	private readonly	BehaviourTree					m_TreeAsset									= null;
		private readonly	BehaviourTree					m_TreeInstance								= null;
		private readonly	AIController					m_Controller								= null;
		private readonly	BTNodeInstanceData[]			m_NodesInstanceData							= null;
		private readonly	RuntimeDataBase[]				m_NodesRuntimeData							= null;
		private readonly	BlackboardInstanceData			m_BlackboardInstanceData					= null;
		private readonly	List<TickableEntry>				m_Tickables									= new List<TickableEntry>();

		//--------------------- Dynamic
		private				BTNodeInstanceData				m_CurrentRunningNode						= null;
		private				bool							m_RunningNodeLocked							= false;
		private				EBehaviourTreeState				m_TreeState									= EBehaviourTreeState.INVALID;
		private				NodeAbortCandidateRequest		m_CandidateForAbort							= null;

		//--------------------- Public
		public				uint							UniqueId									=> m_UniqueId;
	//	public				BehaviourTree					TreeAsset									=> m_TreeAsset;
		public				BehaviourTree					TreeInstance								=> m_TreeInstance;
		public				AIController					Controller									=> m_Controller;
		public				BTNodeInstanceData[]			NodesInstanceData							=> m_NodesInstanceData;
		public				BTNodeInstanceData				RootNode									=> m_NodesInstanceData.At(0);
		public				BlackboardInstanceData			BlackboardInstanceData						=> m_BlackboardInstanceData;
		public				NodeAbortCandidateRequest		CandidateForAbort							=> m_CandidateForAbort;

		//---------------------
		public				BTNodeInstanceData				CurrentRunningNode							=> m_CurrentRunningNode;
		public				bool							IsRunningNodeLocked							=> m_RunningNodeLocked;
		public				EBehaviourTreeState				TreeState									=> m_TreeState;


		//////////////////////////////////////////////////////////////////////////
		public BehaviourTreeInstanceData(in BehaviourTree InBehaviourTreeAsset, in BehaviourTree InTreeInstance, in AIController InController,
			in BTNodeInstanceData[] InNodesInstanceData, in RuntimeDataBase[] InNodesRuntimeData, out BlackboardInstanceData OutBlackboardInstanceData)
		{
		//	m_TreeAsset = InBehaviourTreeAsset;
			m_TreeInstance = InTreeInstance;
			m_Controller = InController;

			m_NodesInstanceData = InNodesInstanceData;

			m_NodesRuntimeData = InNodesRuntimeData;

			OutBlackboardInstanceData = m_BlackboardInstanceData = Blackboard.CreateInstanceData(InBehaviourTreeAsset.BlackboardAsset, this);
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNodeLocked(in bool bLocked) => m_RunningNodeLocked = bLocked;

		//////////////////////////////////////////////////////////////////////////
		public void SetRunningNode(in BTNodeInstanceData InNode) => m_CurrentRunningNode = InNode;


		//////////////////////////////////////////////////////////////////////////
		public void AddTickable(in BTNodeInstanceData InTickableData, in System.Action<BTNodeInstanceData, float> InOnTick)
		{
			m_Tickables.Add(new TickableEntry(InTickableData, InOnTick));
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveTickable(BTNodeInstanceData InTickableData)
		{
			if (m_Tickables.TryFind(out TickableEntry _, out int index, entry => entry.NodeInstanceData == InTickableData))
			{
				m_Tickables.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void UpdateTickables(float InDeltaTime)
		{
			// Reverse iteration = low priority -> high priority
			for (int i = m_Tickables.Count - 1; i >= 0; --i)
			{
				m_Tickables[i].Tick(InDeltaTime);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void SetCandidateForAbort(in BTNodeInstanceData InCandidate, in System.Action<BTNodeInstanceData> InOnRequestAccepted)
		{
			m_CandidateForAbort = new NodeAbortCandidateRequest(InCandidate, InOnRequestAccepted);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveCandidateForAbort()
		{
			m_CandidateForAbort = null;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetTreeState(in EBehaviourTreeState InNewState) => m_TreeState = InNewState;

		//////////////////////////////////////////////////////////////////////////
		public T GetRuntimeData<T>(uint InNodeIndex) where T : RuntimeDataBase => m_NodesRuntimeData.At(InNodeIndex) as T;
	}
}