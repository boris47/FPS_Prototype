
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
		ABORTING,		// start aborting
		ABORTED,        // finished aborting = failure
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


		public static bool IsFinished(in BTNode InNode) => InNode.NodeState == EBTNodeState.SUCCEEDED || InNode.NodeState == EBTNodeState.FAILED || InNode.NodeState == EBTNodeState.ABORTED;


		//////////////////////////////////////////////////////////////////////////
		protected void SetNodeState(in EBTNodeState InNewState) => m_NodeState = InNewState;

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
				BehaviourTree.SetRunningNode(this);
				m_NodeState = OnActivation();
				OnNodeActivation(this);
				CustomAssertions.IsTrue(m_NodeState != EBTNodeState.INACTIVE);
			}

			if (m_NodeState == EBTNodeState.RUNNING)
			{
				m_NodeState = OnUpdate();
			}

			if (m_NodeState == EBTNodeState.ABORTING)
			{
				m_NodeState = OnUpdateAborting();
				CustomAssertions.IsTrue(m_NodeState == EBTNodeState.ABORTING || m_NodeState == EBTNodeState.ABORTED);
			}

			if (BTNode.IsFinished(this))
			{
				OnNodeTermination(this);
				OnTerminate();

				if (m_NodeState != EBTNodeState.ABORTED)
				{
					BehaviourTree.SetRunningNode(Parent as BTNode);
				}
			}
			return m_NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void BeginAbortNode()
		{
			// 
			m_NodeState = EBTNodeState.ABORTING;

			OnBeginAbortNode();
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
		protected virtual EBTNodeState OnActivation() => EBTNodeState.RUNNING;
		protected virtual EBTNodeState OnUpdate() => EBTNodeState.SUCCEEDED;
		protected virtual EBTNodeState OnUpdateAborting() => EBTNodeState.ABORTED;
		protected virtual void OnTerminate() { }
		protected virtual void OnBeginAbortNode() { }
		protected virtual void OnNodeReset() { }
	}
}
