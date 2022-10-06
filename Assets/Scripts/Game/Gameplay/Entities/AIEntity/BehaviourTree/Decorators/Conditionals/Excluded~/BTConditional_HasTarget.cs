using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	[BTNodeDetails("Has Target", "Evaluate if owner has a 'Target'")]
	public sealed class BTConditional_HasTarget : BTConditional
	{
		[SerializeField, ToNodeInspector("Target To Evaluate")]
		private				BlackboardEntryKey					m_BlackboardKey			= null;


		//---------------------
		private				bool								m_PreviousResult		= false;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			Utils.CustomAssertions.IsNotNull(m_BlackboardKey);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation()
		{
			bool OutResult = false;

			if (BehaviourTree.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_TargetEntity BB_Target))
			{
				OutResult = BB_Target.Value.IsNotNull();
			}
			return OutResult;
		}

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
		public override void UpdateTickable(in float InDeltaTime)
		{
			base.UpdateTickable(InDeltaTime);

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
	}
}
