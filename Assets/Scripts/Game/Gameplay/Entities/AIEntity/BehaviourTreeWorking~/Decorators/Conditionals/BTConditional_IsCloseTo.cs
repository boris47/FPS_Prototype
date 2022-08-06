using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public sealed class BTConditional_IsCloseTo : BTConditional
	{
		private const float s_MinRange = 0.001f;

		[System.Serializable]
		private enum ESpace
		{
			BiDimensional,
			ThreeDimensional,
		};

		[SerializeField, ToNodeInspector("Position To Evaluate")]
		private			BlackboardEntryKey					m_BlackboardKey			= null;

		[SerializeField, Min(s_MinRange), ToNodeInspector()]
		private			float								m_ValidRange			= 1f;

		[SerializeField, ToNodeInspector("Dimension")]
		private			ESpace								m_Space					= ESpace.ThreeDimensional;

		//---------------------
		private			bool								m_PreviousResult		= false;


		public override string								NodeName				=> "Conditional by Position";
		public override string								NodeInfo				=> "Evaluate if owner is close enough to 'Position'";

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

			Vector3? position = null;
			if (m_BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_PositionToEvaluate BB_Position))
			{
				position = BB_Position.Value;
			}
			else if (m_BehaviourTree.Owner.Blackboard.TryGetEntry(m_BlackboardKey, out BBEntry_EntityToEvaluate BB_TargetSeen))
			{
				position = BB_TargetSeen.Value.Body.position;
			}

			if (position.HasValue)
			{
				float squaredRange = m_ValidRange * m_ValidRange;
				switch (m_Space)
				{
					case ESpace.BiDimensional:
					{
						OutResult = m_BehaviourTree.Owner.transform.position.DistanceXZSqr(position.Value) < squaredRange;
						break;
					}
					case ESpace.ThreeDimensional:
					default:
					{
						OutResult = m_BehaviourTree.Owner.transform.position.DistanceSqr(position.Value) < squaredRange;
						break;
					}
				}
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
			if (m_AbortType != EAbortType.None && GameManager.CyclesEvents.IsNotNull())
			{
				GameManager.CyclesEvents.OnThink -= OnThink;
			}
		}
	}
}
