using System.Linq;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Root Node", "The root node")]
	public sealed partial class BTRootNode : BTNode, IParentNode
	{
		[SerializeField, HideInInspector]
		private			BTNode			m_Child					= null;

		[SerializeField, ToNodeInspector(bShowLabel: true)]
		private			bool			m_MustRepeat			= false;

		//---------------------
		public			BTNode			Child					=> m_Child;

		public			List<BTNode>	Children
		{
			get
			{
				List<BTNode> children = new List<BTNode>();
				if (m_Child.IsNotNull())
				{
					children.Add(m_Child);
				}
				return children;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetChild(in BTNode child) => m_Child = child;

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTRootNode;
			node.m_Child = m_Child.IsNotNull() ? m_Child.CreateInstance(node) : null;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdate(in float InDeltaTime)
		{
			EBTNodeState OutState = m_Child.IsNotNull() ? EBTNodeState.RUNNING : EBTNodeState.SUCCEEDED;
			if (m_Child.IsNotNull())
			{
				OutState = m_Child.UpdateNode(InDeltaTime);
				if (BTNode.IsFinished(m_Child) && m_MustRepeat)
				{
					m_Child.ResetNode();
					OutState = EBTNodeState.RUNNING;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdateAborting(in float InDeltaTime) => m_Child.IsNotNull() ? m_Child.NodeState : EBTNodeState.ABORTED;

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset()
		{
			base.OnNodeReset();

			if (m_Child.IsNotNull())
			{
				m_Child.ResetNode();
			}
		}
	}
}
