
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public interface IParentNode
	{
		List<BTNode> Children { get; }

		void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState);
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
		protected		EBTNodeState			m_NodeState				= EBTNodeState.INACTIVE;

		[SerializeField, ReadOnly]
		private			BTNode					m_Parent				= null;

		[SerializeField, ReadOnly]
		private			uint					m_NodeIndex				= 0u;

		[SerializeField, ReadOnly]
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
		public event System.Action<BTNode>		OnNodeUpdate			= delegate { };
		public event System.Action<BTNode>		OnNodeTermination		= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public BTNode CloneInstance(in BTNode InParentCloned = null)
		{
			var newInstance = Instantiate(this);
			newInstance.m_NodeState = EBTNodeState.INACTIVE;
			newInstance.m_Parent = InParentCloned;
			newInstance.m_NodeState = m_NodeState;
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
		public bool TryActivation()
		{
			if (CustomAssertions.IsTrue(m_NodeState == EBTNodeState.INACTIVE))
			{
				// Added to visited node because the next call could add it's child/children first
				m_BehaviourTree.AddVisitedNode(this);
				if (OnTryActivation())
				{
					m_NodeState = EBTNodeState.RUNNING;
					OnNodeActivation(this);
				}
				else
				{
					m_BehaviourTree.RemoveVisitedNode(this);
				}
			}
			return m_NodeState == EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Should be called only inside Behaviour Tree class </summary>
		public EBTNodeState OnUpdate()
		{
			if (CustomAssertions.IsTrue(m_NodeState != EBTNodeState.INACTIVE))
			{
				if (m_NodeState == EBTNodeState.RUNNING)
				{
					m_NodeState = PreUpdate();
				}

				if (m_NodeState == EBTNodeState.RUNNING)
				{
					m_NodeState = Update();
					OnNodeUpdate(this);
				}

				if (m_NodeState == EBTNodeState.RUNNING)
				{
					m_NodeState = PostUpdate();
				}

				if (m_NodeState != EBTNodeState.RUNNING)
				{
					// Really terminate this node (Clean-up)
					OnTerminate(false);
					// Remove from tree execution stack
					m_BehaviourTree.RemoveVisitedNode(this);
					// Notify parent
					Parent?.OnChildFinished(this, m_NodeState);
				}
			}
			return m_NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void AbortNode() => OnTerminate(/*bIsAbort*/true);

		//////////////////////////////////////////////////////////////////////////
		public virtual void ResetNode()
		{
			m_NodeState = EBTNodeState.INACTIVE;
		}

		//////////////////////////////////////////////////////////////////////////
	//	public abstract List<BTNode> GetChildren();
		protected abstract void CopyDataToInstance(in BTNode InNewInstance);
		protected abstract void OnAwakeInternal(in BehaviourTree InBehaviourTree);
		protected virtual bool OnTryActivation() => true;
		protected virtual EBTNodeState PreUpdate() => EBTNodeState.RUNNING;
		protected virtual EBTNodeState Update() => EBTNodeState.RUNNING;
		protected virtual EBTNodeState PostUpdate() => EBTNodeState.RUNNING;
		protected virtual void OnTerminate(in bool bIsAbort)
		{
			// Notify externals
			OnNodeTermination(this);

			if (bIsAbort)
			{
				CustomAssertions.IsTrue(m_NodeState == EBTNodeState.RUNNING);
			}
			else
			{
				CustomAssertions.IsTrue(m_NodeState != EBTNodeState.RUNNING);
			}
		}
	}
}
