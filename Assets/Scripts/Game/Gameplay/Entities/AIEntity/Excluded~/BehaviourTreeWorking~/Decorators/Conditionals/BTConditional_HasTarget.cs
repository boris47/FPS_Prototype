using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public sealed class BTConditional_HasTarget : BTConditional
	{
		private const float s_MinRange = 0.001f;

		[System.Serializable]
		private enum ESpace
		{
			BiDimensional,
			ThreeDimensional,
		};

		[SerializeField, ToNodeInspector("Target To Evaluate")]
		private			BlackboardEntryKey					m_BlackboardKey			= null;

		[SerializeField, Min(s_MinRange), ToNodeInspector()]
		private			float								m_ValidRange			= 1f;

		[SerializeField, ToNodeInspector("Dimension")]
		private			ESpace								m_Space					= ESpace.ThreeDimensional;

		//---------------------
		private			bool								m_PreviousResult		= false;

		public override string								NodeName				=> "Conditional by Target";
		public override string								NodeInfo				=> "Evaluate if owner has a 'Target'";

		/*
		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			base.CopyDataToInstance(InNewInstance);
			var node = InNewInstance as BTConditional_BlackboardValue;
			node.m_BlackboardKey = m_BlackboardKey;
		}
		*/

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAwakeInternal(in BehaviourTree InBehaviourTree)
		{
			base.OnAwakeInternal(InBehaviourTree);

			CustomAssertions.IsNotNull(m_BlackboardKey);
			CustomAssertions.IsTrue(m_ValidRange >= s_MinRange);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation()
		{
			bool OutResult = false;

			if (m_BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_EntityToEvaluate BB_TargetSeen))
			{
				OutResult = BB_TargetSeen.Value.IsNotNull();
			}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBecomeRelevant()
		{
			if (m_AbortType != EAbortType.None && CustomAssertions.IsNotNull(GameManager.CyclesEvents))
			{
				GameManager.CyclesEvents.OnThink += OnThink;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnThink()
		{
			bool bCurrentEvaluationResult = GetEvaluation();
			if (CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			{
				// On conditional lost will fail and this node will abort returning flow control to parent
				if (m_PreviousResult && !bCurrentEvaluationResult)
				{
					AbortSelf();
				}

				// On condition met request the tree to abort running branch and move to this node and run it
				else if (!m_PreviousResult && bCurrentEvaluationResult)
				{
					// We expect this node not to be in running state
					if (CustomAssertions.IsTrue(NodeState != EBTNodeState.RUNNING))
					{
						AbortLowPriority();
					}
				}
				m_PreviousResult = bCurrentEvaluationResult;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnCeaseRelevant()
		{
			if (GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnThink -= OnThink;
			}
		}
	}
}
