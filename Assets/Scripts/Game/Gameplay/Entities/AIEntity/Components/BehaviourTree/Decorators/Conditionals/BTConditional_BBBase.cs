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
			if (ChildAsset.IsNotNull() && BlackboardKey.IsValid() && m_AbortType != EAbortType.None)
			{
				InThisNodeInstanceData.AddObserver(BlackboardKey, OnKeyValueChange);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void StopObserve(in BTNodeInstanceData InThisNodeInstanceData)
		{
			{
				InThisNodeInstanceData.RemoveObserver(BlackboardKey, OnKeyValueChange);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private EOnChangeDelExecutionResult OnKeyValueChange(in BlackboardInstanceData InBlackboardInstance, in BlackboardEntryKey InKey, in EBlackboardValueOp InOperation)
		{
			EOnChangeDelExecutionResult OutResult = EOnChangeDelExecutionResult.LEAVE;

			BTNodeInstanceData thisNodeInstanceData = InBlackboardInstance.BehaviourTreeInstanceData.NodesInstanceData[NodeIndex];

			if (Utils.CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			if (ChildAsset.IsNotNull() && Utils.CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			if (Child.IsNotNull())
			{
				// On value removed(set default) conditional will fail and this node will abort returning flow control to parent
				if (InOperation == EBlackboardValueOp.REMOVE)
				BTNodeInstanceData thisNodeInstanceData = InBlackboardInstance.BehaviourTreeInstanceData.NodesInstanceData[NodeIndex];
				
				// On value removed(set default) conditional will fail and this node will abort returning flow control to parent
				if (InOperation == EBlackboardValueOp.REMOVE)
				if (Utils.CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
				{
					BTNodeInstanceData thisNodeInstanceData = InBlackboardInstance.BehaviourTreeInstanceData.NodesInstanceData[NodeIndex];

					// On value removed(set default) conditional will fail and this node will abort returning flow control to parent
					if (InOperation == EBlackboardValueOp.REMOVE)
					{
						if (Utils.CustomAssertions.IsTrue(thisNodeInstanceData.NodeState == EBTNodeState.RUNNING))
						{
							BTNodeInstanceData childInstanceData = GetChildInstanceData(thisNodeInstanceData, Child);
							Child.AbortAndResetNode(childInstanceData);
							BTNodeInstanceData childInstanceData = GetNodeInstanceData(thisNodeInstanceData, ChildAsset);
							ChildAsset.AbortAndResetNode(GetNodeInstanceData(thisNodeInstanceData, ChildAsset));
							OutResult = AbortSelf(thisNodeInstanceData);
						}
					}

					// On value changed (from value to value (not default one)), if guard is set, abort running child, reset and restart it
					else if (InOperation == EBlackboardValueOp.CHANGE)
					{
						// If this node is running then process the change event
						if (thisNodeInstanceData.NodeState == EBTNodeState.RUNNING)
						{
							if (m_ResetOnChange)
							{
								// Abort and reset child
								BTNodeInstanceData childInstanceData = GetChildInstanceData(thisNodeInstanceData, Child);
								Child.AbortAndResetNode(childInstanceData);

								// Set this node ar running one to update child
								thisNodeInstanceData.BehaviourTreeInstanceData.SetRunningNode(thisNodeInstanceData);
							}
						}
						// Otherwise this could mean that the node has failed previously and now observer is live so we want to remove it
						else
						{
							OutResult = EOnChangeDelExecutionResult.REMOVE;
						}
					}

							childInstanceData.BehaviourTreeInstanceData.SetRunningNode(childInstanceData);
							childInstanceData.SetAsRunningNode();
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
				// No point to keep observing if abort type is None
				else
				{
					OutResult = EOnChangeDelExecutionResult.REMOVE;
				}
			}
			// At this point there should be a child but whenever this condition is not met, we remove the observer
			else
			{
				OutResult = EOnChangeDelExecutionResult.REMOVE;
			}
			return OutResult;
		}
	}
}
