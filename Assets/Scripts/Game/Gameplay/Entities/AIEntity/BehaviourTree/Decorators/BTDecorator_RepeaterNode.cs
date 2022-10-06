using System.Collections;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	/// <summary>
	/// Repeater composite node. <br/>
	/// Repeater Nodes repeats their own child execution n-times ignoring the result. <br/>
	/// </summary>
	[BTNodeDetails("Repeater", "Repeats its child execution n-times ignoring the result")]
	public partial class BTDecorator_RepeaterNode : BTDecoratorNode
	{
		[SerializeField, Min(0u), Tooltip("[0-4294967295u], 0 (Zero) Means infinite"), ToNodeInspector]
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
		protected override EBTNodeState OnActivation()
		{
			m_CurrentRepeatCount = 0u;

			return EBTNodeState.RUNNING;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EBTNodeState OnUpdate(in float InDeltaTime)
		{
			EBTNodeState OutState = EBTNodeState.RUNNING;
			switch (Child.UpdateNode(InDeltaTime))
			{
				case EBTNodeState.FAILED:
				case EBTNodeState.SUCCEEDED:
				{
					if (m_RepeatCount > 0)
					{
						m_CurrentRepeatCount++;
						if (m_CurrentRepeatCount >= m_RepeatCount)
						{

							OutState = EBTNodeState.SUCCEEDED;
						}
						else
						{
							Child.ResetNode();
						}
					}
					else
					{
						Child.ResetNode();
					}
					break;
				}
			}
			return OutState;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnNodeReset()
		{
			m_CurrentRepeatCount = 0u;

			Child.ResetNode();
		}
	}
}
