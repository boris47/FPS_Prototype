using UnityEngine;


namespace Entities.AI.Components.Behaviours
{
	[BTNodeDetails("Conditional by Blackboard key", "If a child is assigned verify if condition is satisfied before activate it")]
	public abstract class BTConditional_BBBase : BTConditional
	{
		[SerializeField, ToNodeInspector]
		[Tooltip("On value changed (from value to value (not default one)), if guard is set, abort running child, reset and restart it")]
		private				bool											m_ResetOnChange									= true;

		protected abstract	BlackboardEntryKey								BlackboardKey									{ get; }


		//////////////////////////////////////////////////////////////////////////
		protected override void StartObserve(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (BlackboardKey.IsValid() && m_AbortType != EAbortType.None)
			{
				InThisNodeInstanceData.AddObserver(BlackboardKey, ConditionalAbort);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void StopObserve(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (BlackboardKey.IsValid())
			{
				InThisNodeInstanceData.RemoveObserver(BlackboardKey, ConditionalAbort);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private EOnChangeDelExecutionResult ConditionalAbort(in BlackboardInstanceData InBlackboardInstance, in BlackboardEntryKey InKey, in EBlackboardValueOp InOperation)
		{
			EOnChangeDelExecutionResult OutResult = EOnChangeDelExecutionResult.LEAVE;

			BTNodeInstanceData thisNodeInstanceData = InBlackboardInstance.BehaviourTreeInstanceData.NodesInstanceData[NodeIndex];

			if (Utils.CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			{
				// On value removed(set default) conditional will fail and this node will abort returning flow control to parent
				if (InOperation == EBlackboardValueOp.REMOVE)
				{
					OutResult = AbortSelf(thisNodeInstanceData);
				}

				// On value changed (from value to value (not default one)), if guard is set, abort running child, reset and restart it
				else if (InOperation == EBlackboardValueOp.CHANGE)
				{
					if (m_ResetOnChange)
					{
						if (Utils.CustomAssertions.IsTrue(thisNodeInstanceData.NodeState == EBTNodeState.RUNNING))
						{
							BTNodeInstanceData childInstanceData = GetChildInstanceData(thisNodeInstanceData, Child);
							Child.AbortAndResetNode(childInstanceData);

							childInstanceData.BehaviourTreeInstanceData.SetRunningNode(childInstanceData);
						}
					}
				}

				// On value added (from default to a valid value) request the tree to abort running branch and move to this node and run it
				else
				{
					// We expect this node not to be in running state
					if (Utils.CustomAssertions.IsTrue(thisNodeInstanceData.NodeState != EBTNodeState.RUNNING))
					{
						RequestLowPriorityAbort(thisNodeInstanceData);
					}
				}
			}
			return OutResult;
		}
	}
}
