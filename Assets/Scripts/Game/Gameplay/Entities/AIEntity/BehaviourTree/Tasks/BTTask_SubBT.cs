using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Sub Behaviour Tree", "Execute a sub behaviour tree")]
	public sealed class BTTask_SubBT : BTTaskNode
	{
		[SerializeField, ToNodeInspector(bShowLabel: true)]
		public BehaviourTree			m_SubBehaviourTree				= null;


		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			base.CopyDataToInstance(InNewInstance);

			if (m_SubBehaviourTree.IsNotNull())
			{
				if (m_SubBehaviourTree.IsInstance)
				{
					Utils.CustomAssertions.IsTrue(false);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			if (m_SubBehaviourTree.IsNotNull())
			{
				m_SubBehaviourTree = BehaviourTree.CreateInstanceFrom(m_SubBehaviourTree);
				m_SubBehaviourTree.OnAwake(m_SubBehaviourTree.Owner);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnActivation()
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			if (m_SubBehaviourTree.IsNotNull())
			{
				m_SubBehaviourTree.StartTree();
			}
			else
			{
				OutState = EBTNodeState.FAILED;
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in float InDeltaTime) => m_SubBehaviourTree.UpdateTree(InDeltaTime);

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortNodeRequested(in bool bAbortImmediately)
		{
			m_SubBehaviourTree.RootNode.RequestAbortNode(bAbortImmediately);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting(in float InDeltaTime)
		{
			return m_SubBehaviourTree.TreeState == EBehaviourTreeState.STOPPED ? EBTNodeState.ABORTED : EBTNodeState.ABORTING;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{
			if (m_SubBehaviourTree.IsNotNull())
			{
				m_SubBehaviourTree.RootNode.ResetNode();
			}
		}
	}
}
