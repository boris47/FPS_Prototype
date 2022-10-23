using UnityEngine;

namespace Entities.AI.Components
{
	public interface IBlackboardProjector
	{
		Blackboard GetBlackboardAsset();
		void SetBlackboardKey(in BlackboardEntryKey InBlackboardEntryKey);
	}
}

namespace Entities.AI.Components.Behaviours
{

	[System.Serializable]
	[BTNodeDetails("Conditional by Blackboard key", "If a child is assigned verify if condition is satisfied before activate it")]
	public sealed class BTConditional_BlackboardValue : BTConditional, IBlackboardProjector
	{
		[SerializeReference, ToNodeInspector("Blackboard Key")]
		private				BlackboardEntryKey					m_BlackboardKey			= null;

		[SerializeField, ToNodeInspector(bShowLabel: true)]
		[Tooltip("If true on key value change request a child reset by aborting it and re-running it")]
		private				bool								m_ResetOnChange			= true;

		//////////////////////////////////////////////////////////////////////////
		Blackboard IBlackboardProjector.GetBlackboardAsset() => BehaviourTreeAsset.BlackboardAsset;
		//////////////////////////////////////////////////////////////////////////
		void IBlackboardProjector.SetBlackboardKey(in BlackboardEntryKey InBlackboardEntryKey) => m_BlackboardKey = InBlackboardEntryKey;



		//////////////////////////////////////////////////////////////////////////
		private static bool TryGetEntryBase(in BTNodeInstanceData InNodeInstanceData, in BlackboardEntryKey InEntryKey, out BlackboardEntryBase OutEntryBase)
		{
			Blackboard asset = InNodeInstanceData.BehaviourTreeInstanceData.Blackboard.BlackboardAsset;
			BlackboardInstanceData bbInstanceData = InNodeInstanceData.BehaviourTreeInstanceData.Blackboard;
			return asset.TryGetEntryBase(bbInstanceData, InEntryKey, out OutEntryBase);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation(in BTNodeInstanceData InThisNodeInstanceData)
		{
			bool outValue = false;
			if (m_BlackboardKey.IsNotNull() && TryGetEntryBase(InThisNodeInstanceData, m_BlackboardKey, out BlackboardEntryBase OutEntryBase))
			{
				outValue = OutEntryBase.HasValue();
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBecomeRelevant(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_BlackboardKey.IsNotNull() && m_AbortType != EAbortType.None)
			{
				Blackboard asset = InThisNodeInstanceData.BehaviourTreeInstanceData.Blackboard.BlackboardAsset;
				BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.Blackboard;
				asset.AddObserver(bbInstanceData, m_BlackboardKey, ConditionalAbort);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnCeaseRelevant(in BTNodeInstanceData InThisNodeInstanceData)
		{
			if (m_BlackboardKey.IsNotNull())
			{
				Blackboard asset = InThisNodeInstanceData.BehaviourTreeInstanceData.Blackboard.BlackboardAsset;
				BlackboardInstanceData bbInstanceData = InThisNodeInstanceData.BehaviourTreeInstanceData.Blackboard;
				asset.RemoveObserver(bbInstanceData, m_BlackboardKey, ConditionalAbort);
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

				// On value changed (from value to value (not default one)), if guard is set, abort running child, reset and restart child node
				else if (InOperation == EBlackboardValueOp.CHANGE)
				{
					if (m_ResetOnChange)
					{
						if (Utils.CustomAssertions.IsTrue(thisNodeInstanceData.NodeState == EBTNodeState.RUNNING))
						{
							BTNodeInstanceData childInstanceData = GetChildInstanceData(thisNodeInstanceData, Child);
							Child.RequestAbortNode(childInstanceData);
							Child.ResetNode(childInstanceData);
							BehaviourTreeAsset.SetRunningNode(childInstanceData);
						}
					}
				}

				// On value added (from default to a valid value) request the tree to abort running branch and move to this node and run it
				else
				{
					// We expect this node not to be in running state
					if (Utils.CustomAssertions.IsTrue(thisNodeInstanceData.NodeState != EBTNodeState.RUNNING))
					{
						AbortLowPriority(thisNodeInstanceData);
					}
				}
			}
			return OutResult;
		}
	}
}
