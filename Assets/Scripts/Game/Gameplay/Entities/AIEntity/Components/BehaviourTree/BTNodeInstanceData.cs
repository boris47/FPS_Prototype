
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	//////////////////////////////////////////////////////////////////////////
	public class BTNodeInstanceData
	{
		private readonly	BehaviourTreeInstanceData		m_InBehaviourTreeInstanceData	= null;
		private readonly	BlackboardInstanceData			m_BlackboardInstanceData		= null;
		private readonly	BTNode							m_NodeAsset						= null;
		private readonly	BTNode							m_NodeInstance					= null;
		private readonly	BTNodeInstanceData				m_ParentInstanceData			= null;

		//---------------------
		private				EBTNodeState					m_NodeState						= EBTNodeState.INACTIVE;

		//---------------------
		public				BehaviourTreeInstanceData		BehaviourTreeInstanceData		=> m_InBehaviourTreeInstanceData;
		public				BTNode							NodeAsset						=> m_NodeAsset;
		public				BTNode							NodeInstance					=> m_NodeInstance;
		public				BTNodeInstanceData				ParentInstanceData				=> m_ParentInstanceData;
		public				EBTNodeState					NodeState						=> m_NodeState;
		public				AIController					Controller						=> m_InBehaviourTreeInstanceData.Controller;


		//////////////////////////////////////////////////////////////////////////
		public BTNodeInstanceData(in BehaviourTreeInstanceData InTreeInstanceData, in BlackboardInstanceData InBlackboardInstanceData, in BTNode InNodeAsset, in BTNode InNodeInstance, in BTNodeInstanceData InParentInstanceData = null)
		{
			m_InBehaviourTreeInstanceData = InTreeInstanceData;
			m_BlackboardInstanceData = InBlackboardInstanceData;
			m_NodeAsset = InNodeAsset;
			m_NodeInstance = InNodeInstance;
			m_ParentInstanceData = InParentInstanceData;
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetNodeState(in EBTNodeState InNewState)
		{
			m_NodeState = InNewState;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_BlackboardInstanceData.BlackboardAsset.TryGetEntryBase(m_BlackboardInstanceData, InBlackboardKey, out OutEntry);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntry<T>(BlackboardEntryKey InBlackboardKey, out T OutEntry) where T : BlackboardEntryBase, new()
		{
			return m_BlackboardInstanceData.BlackboardAsset.TryGetEntry(m_BlackboardInstanceData, InBlackboardKey, out OutEntry);
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			m_BlackboardInstanceData.BlackboardAsset.AddObserver(m_BlackboardInstanceData, InBlackboardKey, InObserverDelegate);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			m_BlackboardInstanceData.BlackboardAsset.RemoveObserver(m_BlackboardInstanceData, InBlackboardKey, InObserverDelegate);
		}
	}
}