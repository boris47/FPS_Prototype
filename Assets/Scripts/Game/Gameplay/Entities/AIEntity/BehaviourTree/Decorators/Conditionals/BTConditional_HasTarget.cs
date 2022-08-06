using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	[BTNodeDetails("Conditional by Target", "Evaluate if owner has a 'Target'")]
	public sealed class BTConditional_HasTarget : BTConditional
	{
		private const		float								s_MinRange				= 0.001f;

		[System.Serializable]
		private enum ESpace
		{
			BiDimensional,
			ThreeDimensional,
		};

		[SerializeField, ToNodeInspector("Target To Evaluate")]
		private				BlackboardEntryKey					m_BlackboardKey			= null;

		[SerializeField, Min(s_MinRange), ToNodeInspector()]
		private				float								m_ValidRange			= 1f;


		//---------------------
		private				bool								m_PreviousResult		= false;


		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			Utils.CustomAssertions.IsNotNull(m_BlackboardKey);
			Utils.CustomAssertions.IsTrue(m_ValidRange >= s_MinRange);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation()
		{
			bool OutResult = false;

			if (BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_EntityToEvaluate BB_TargetSeen))
			{
				OutResult = BB_TargetSeen.Value.IsNotNull();
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
	}
}
