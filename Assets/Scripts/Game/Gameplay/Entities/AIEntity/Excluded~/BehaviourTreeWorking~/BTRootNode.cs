using System.Linq;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Entities.AI.Components.Behaviours
{
	public sealed partial class BTRootNode : BTNode, IParentNode
	{
		public override string NodeName => "Root";
		public override string NodeInfo => "The root node";

		//---------------------
		[SerializeField, ReadOnly/*, HideInInspector*/]
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
			node.m_Child = m_Child?.CloneInstance(node) ?? null;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdate()
		{
			EBTNodeState OutState = Child.IsNotNull() ? EBTNodeState.RUNNING : EBTNodeState.SUCCEEDED;
			if (Child.IsNotNull())
			{
				OutState = Child.Update();
				if (m_MustRepeat)
				{
					Child.ResetNode();
					OutState = EBTNodeState.RUNNING;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override EBTNodeState OnUpdateAborting() => Child.NodeState;

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnNodeReset()
		{
			base.OnNodeReset();

			Child?.ResetNode();
		}
	}
}
