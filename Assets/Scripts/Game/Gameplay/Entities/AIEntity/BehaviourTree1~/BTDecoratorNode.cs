using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public abstract class BTDecoratorNode : BTNode, IParentNode
	{
		[SerializeField, ReadOnly/*, HideInInspector*/]
		private				BTNode			m_Child					= null;

		//---------------------
		public				BTNode			Child					=> m_Child;

		public List<BTNode> Children
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
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			m_Child?.OnAwake(InBehaviourTree);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool OnTryActivation() => Child.IsNotNull() && Child.TryActivation();

		//////////////////////////////////////////////////////////////////////////
		public abstract void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState);

		//////////////////////////////////////////////////////////////////////////
		public override void ResetNode()
		{
			m_Child?.ResetNode();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate(in bool bIsAbort)
		{
			if (bIsAbort)
			{

			}
			else
			{

			}
			base.OnTerminate(bIsAbort);
		}
	}
}
