
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public interface IParentNode
	{
		List<BTNode> Children { get; }

//		void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState);
	}

	[System.Serializable]
	public enum EBTNodeState
	{
		INACTIVE,		// not has not state defined
		SUCCEEDED,      // finished as success
		FAILED,         // finished as failure
//		ABORTED,        // finished aborting = failure
		RUNNING,        // not finished yet
	}

	public abstract partial class BTNode : ScriptableObject
	{
		[SerializeField, ReadOnly]
		private			EBTNodeState			m_NodeState				= EBTNodeState.INACTIVE;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BTNode					m_Parent				= null;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			uint					m_NodeIndex				= 0u;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		protected		BehaviourTree			m_BehaviourTree			= null;

		//---------------------
		public abstract string					NodeName				{ get; }
		public abstract string					NodeInfo				{ get; }

		//---------------------
		public EBTNodeState						NodeState				=> m_NodeState;
		public IParentNode						Parent					=> m_Parent as IParentNode;
		public uint								NodeIndex				=> m_NodeIndex;
		public BehaviourTree					BehaviourTree			=> m_BehaviourTree;

		public event System.Action<BTNode>		OnNodeActivation		= delegate { };
		public event System.Action<BTNode>		OnNodeTermination		= delegate { };


		//////////////////////////////////////////////////////////////////////////
		protected void SetNodeState(in EBTNodeState InNewStae) => m_NodeState = InNewStae;

		//////////////////////////////////////////////////////////////////////////
		public BTNode CloneInstance(in BTNode InParentCloned = null)
		{
			var newInstance = Instantiate(this);
			newInstance.m_NodeState = EBTNodeState.INACTIVE;
			newInstance.m_Parent = InParentCloned;
#if UNITY_EDITOR
			newInstance.m_Guid = m_Guid;
			newInstance.m_Position = m_Position;
			newInstance.m_HasBreakpoint = m_HasBreakpoint;
#endif
			CopyDataToInstance(newInstance);
			return newInstance;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> For runtime setup purpose </summary>
		public void OnAwake(in BehaviourTree InBehaviourTree)
		{
			m_BehaviourTree = InBehaviourTree;

			OnAwakeInternal(InBehaviourTree);
		}

		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState Update()
		{
			if (m_NodeState == EBTNodeState.INACTIVE)
			{
				m_NodeState = EBTNodeState.RUNNING;
				BehaviourTree.SetRunningNode(this);
				OnActivation();
				OnNodeActivation(this);
			}

			if (m_NodeState == EBTNodeState.RUNNING)
			{
				m_NodeState = OnUpdate();
			}

			if (m_NodeState == EBTNodeState.SUCCEEDED || m_NodeState == EBTNodeState.FAILED)
			{
				OnTerminate(false);
				OnNodeTermination(this);

				BehaviourTree.SetRunningNode(Parent as BTNode);
			}
			return m_NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void AbortNode()
		{
			OnTerminate(/*bIsAbort*/true);
			OnNodeTermination(this);
		}

		//////////////////////////////////////////////////////////////////////////
		public void ResetNode()
		{
			m_NodeState = EBTNodeState.INACTIVE;
			OnNodeReset();
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void CopyDataToInstance(in BTNode InNewInstance) { }
		protected virtual void OnAwakeInternal(in BehaviourTree InBehaviourTree) { }
		protected abstract void OnActivation();
		protected abstract EBTNodeState OnUpdate();
		protected abstract void OnTerminate(in bool bIsAbort);
		protected abstract void OnNodeReset();
	}
}
