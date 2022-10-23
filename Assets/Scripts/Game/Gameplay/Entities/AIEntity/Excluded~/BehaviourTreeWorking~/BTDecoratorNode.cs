using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract class BTDecoratorNode : BTNode, IParentNode
	{
		[SerializeField, /*ReadOnly*/HideInInspector]
		private				BTNode			m_Child					= null;

		//---------------------
		public				BTNode			Child					=> m_Child;

		public				List<BTNode>	Children
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
		public void SetChild(BTNode child) => m_Child = child;

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTDecoratorNode;
			node.m_Child = m_Child?.CloneInstance(node) ?? null;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdateAborting() => m_Child?.NodeState ?? EBTNodeState.ABORTED;

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBeginAbortNode()
		{
			base.OnBeginAbortNode();

			m_Child?.BeginAbortNode();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{
			base.OnNodeReset();

			m_Child?.ResetNode();
		}
	}
}
