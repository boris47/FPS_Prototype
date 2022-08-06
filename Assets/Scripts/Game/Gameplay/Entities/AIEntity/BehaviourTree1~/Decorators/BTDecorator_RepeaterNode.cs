using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	public partial class BTDecorator_RepeaterNode : BTDecoratorNode
	{
		public override string NodeName => "Repeater";
		public override string NodeInfo => "Repeats its child execution n-times ignoring the result";

		[SerializeField, Min(0u), Tooltip("[0-4294967295u], 0 (Zero) Means infinite")]
		protected uint m_RepeatCount = 1u;

		[SerializeField, ReadOnly]
		protected uint m_CurrentRepeatCount = 0u;

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			base.CopyDataToInstance(InNewInstance);
			var node = InNewInstance as BTDecorator_RepeaterNode;
			node.m_RepeatCount = m_RepeatCount;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);
			m_CurrentRepeatCount = 0u;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool OnTryActivation()
		{
			m_CurrentRepeatCount = 0;
			return base.OnTryActivation();
		}

		//////////////////////////////////////////////////////////////////////////
		public override void OnChildFinished(in BTNode InNode, in EBTNodeState InChildState)
		{
			m_NodeState = EBTNodeState.RUNNING;
			if (m_RepeatCount > 0)
			{
				if (m_CurrentRepeatCount + 1 >= m_RepeatCount)
				{
					m_NodeState = EBTNodeState.SUCCEEDED;
				}
				else
				{
					Child.ResetNode();
					Child.TryActivation();
					m_CurrentRepeatCount++;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void ResetNode()
		{
			m_CurrentRepeatCount = 0;
		}
	}
}
