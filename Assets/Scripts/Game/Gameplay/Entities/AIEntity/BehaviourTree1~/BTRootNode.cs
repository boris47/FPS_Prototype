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
		public void SetChild(BTNode child) => m_Child = child;

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void CopyDataToInstance(in BTNode InNewInstance)
		{
			var node = InNewInstance as BTRootNode;
			node.m_Child = m_Child?.CloneInstance(node) ?? null;
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			m_Child?.OnAwake(InBehaviourTree);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool OnTryActivation() => Child.IsNotNull() && Child.TryActivation();

		//////////////////////////////////////////////////////////////////////////
		public void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState)
		{
			m_NodeState = InChildState;
		}
	}
}
