
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract partial class BTNode : ScriptableObject, IBTNodeTickable
	{
		[SerializeField, ReadOnly]
		private			EBTNodeState			m_NodeState				= EBTNodeState.INACTIVE;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BTNode					m_Parent				= null;

		[SerializeField, ReadOnly/*HideInInspector*/]
		private			uint					m_NodeIndex				= 0u;

		[SerializeField, /*ReadOnly*/ HideInInspector]
		private			BehaviourTree			m_BehaviourTree			= null;

		//---------------------
		public EBTNodeState						NodeState				=> m_NodeState;
		public IParentNode						Parent					=> m_Parent as IParentNode;
		public uint								NodeIndex				=> m_NodeIndex;
		public BehaviourTree					BehaviourTree			=> m_BehaviourTree;
		public event System.Action<BTNode>		OnNodeActivation		= delegate { };
		public event System.Action<BTNode>		OnNodeTermination		= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public static bool IsFinished(in BTNode InNode) => InNode.NodeState == EBTNodeState.SUCCEEDED || InNode.NodeState == EBTNodeState.FAILED || InNode.NodeState == EBTNodeState.ABORTED;


		//////////////////////////////////////////////////////////////////////////
		protected void SetNodeState(in EBTNodeState InNewState) => m_NodeState = InNewState;

		//////////////////////////////////////////////////////////////////////////
		public BTNode CloneInstance(in BTNode InParentCloned = null)
		{
			Utils.CustomAssertions.IsTrue((this is BTRootNode && InParentCloned == null) || true);
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

		private bool bCanTerminate = true;
		//////////////////////////////////////////////////////////////////////////
		public EBTNodeState Update()
		{
			if (m_NodeState == EBTNodeState.INACTIVE)
			{
				BehaviourTree.SetRunningNode(this);
				m_NodeState = OnActivation();
				bCanTerminate = m_NodeState != EBTNodeState.FAILED;
				OnNodeActivation(this);
				Utils.CustomAssertions.IsTrue(m_NodeState != EBTNodeState.INACTIVE);
			}

			if (m_NodeState == EBTNodeState.RUNNING)
			{
				m_NodeState = OnUpdate();
			}

			if (m_NodeState == EBTNodeState.ABORTING)
			{
				m_NodeState = OnUpdateAborting();
				Utils.CustomAssertions.IsTrue(m_NodeState == EBTNodeState.ABORTING || m_NodeState == EBTNodeState.ABORTED);
			}

			if (BTNode.IsFinished(this))
			{
				if (bCanTerminate)
				{
					OnNodeTermination(this);
					OnTerminate();
				}

				if (m_NodeState != EBTNodeState.ABORTED)
				{
					BehaviourTree.SetRunningNode(Parent as BTNode);
				}
			}
			return m_NodeState;
		}

		//////////////////////////////////////////////////////////////////////////
		public virtual void UpdateFrame(in float InDeltaTime)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public virtual void UpdateFixed()
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestAbortNode(in bool bAbortImmediately)
		{
			m_NodeState = EBTNodeState.ABORTING;

			OnAbortNodeRequested(bAbortImmediately);

			if (bAbortImmediately)
			{
				m_NodeState = EBTNodeState.ABORTED;
			}
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
		protected virtual void OnAbortNodeRequested(in bool bAbortImmediately) { }
		protected virtual void OnNodeReset() { }
	}
}
