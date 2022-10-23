using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	[BTNodeDetails("Has memory of", "Evaluate if owner has a specific 'Memory'")]
	public sealed class BTConditional_HasMemory : BTConditional
	{
		[SerializeField, ToNodeInspector("Entity To Evaluate")]
		private			MemoryIdentifier					m_MemoryIdentifier		= null;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			Utils.CustomAssertions.IsNotNull(m_MemoryIdentifier);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation()
		{
			return BehaviourTree.Owner.BrainComponent.MemoryComponent.HasMemoryOf(m_MemoryIdentifier);
		}
		/*
		//////////////////////////////////////////////////////////////////////////
		protected override void OnBecomeRelevant()
		{
			if (m_AbortType != EAbortType.None)
			{
				BehaviourTree.AddTickableNode(this);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnCeaseRelevant()
		{
			BehaviourTree.RemoveTickableNode(this);
		}

		//////////////////////////////////////////////////////////////////////////
		public override void UpdateFixed()
		{
			base.UpdateFixed();

			if (Utils.CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			{
				bool bCurrentEvaluationResult = GetEvaluation();

				// On conditional lost will fail and this node will abort returning flow control to parent
				if (m_PreviousResult && !bCurrentEvaluationResult)
				{
					AbortSelf();
				}

				// On condition met request the tree to abort running branch and move to this node and run it
				else if (!m_PreviousResult && bCurrentEvaluationResult)
				{
					// We expect this node not to be in running state
					if (Utils.CustomAssertions.IsTrue(NodeState != EBTNodeState.RUNNING))
					{
						AbortLowPriority();
					}
				}
				m_PreviousResult = bCurrentEvaluationResult;
			}
		}
		*/
	}
}
