using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public sealed class BTConditional_BlackboardValue : BTConditional
	{
		[SerializeField, ToNodeInspector("BB Key")]
		private BlackboardEntryKey m_BlackboardKey = null;

		[SerializeField, ToNodeInspector(bShowLabel: true)]
		private bool m_ResetOnChange = true;

		public override string NodeName => "Conditional by Blackboard key";
		public override string NodeInfo => "If a child is assigned verify if condition is satisfied before activate it";

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTNode InNewInstance)
		{
			base.CopyDataToInstance(InNewInstance);
			var node = InNewInstance as BTConditional_BlackboardValue;
			node.m_BlackboardKey = m_BlackboardKey;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override bool GetEvaluation()
		{
			if (m_BlackboardKey && m_BehaviourTree.Owner.Blackboard.TryGetEntryBase(m_BlackboardKey, out BlackboardEntryBase baseKey))
			{
				return baseKey.HasValue();
			}
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnBecomeRelevant()
		{
			if (m_BlackboardKey && m_AbortType != EAbortType.None)
			{
				m_BehaviourTree.Owner.Blackboard.AddObserver(m_BlackboardKey, ConditionalAbort);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnCeaseRelevant()
		{
			if (m_BlackboardKey)
			{
				m_BehaviourTree.Owner.Blackboard.RemoveObserver(m_BlackboardKey, ConditionalAbort);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private EOnChangeDelExecutionResult ConditionalAbort(in BlackboardEntryKey InKey, in EBlackboardValueOp InOperation)
		{
			EOnChangeDelExecutionResult OutResult = EOnChangeDelExecutionResult.LEAVE;

			if (CustomAssertions.IsTrue(m_AbortType != EAbortType.None))
			{
				// On value removed(set default) conditional will fail and this node will abort returning flow control to parent
				if (InOperation == EBlackboardValueOp.REMOVE)
				{
					OutResult = AbortSelf();
				}

				// On value changed (from value to value (not default one)), if guard is set, abort running child, reset and restart this node
				else if (InOperation == EBlackboardValueOp.CHANGE)
				{
					if (m_ResetOnChange)
					{
						Child.AbortNode();

						Child.ResetNode();

						BehaviourTree.SetRunningNode(Child);
					}
				}

				// On value added (from default to a valid value) request the tree to abort running branch and move to this node and run it
				else
				{
					// We expect this node not to be in running state
					if (CustomAssertions.IsTrue(NodeState != EBTNodeState.RUNNING))
					{
						AbortLowPriority();
					}
				}
			}
			return OutResult;
		}
	}
}
