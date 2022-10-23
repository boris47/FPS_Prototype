using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public sealed class BTConditional_BlackboardValue : BTConditional
	{
		[SerializeField]
		private BlackboardEntryKey m_BlackboardKey = null;

		//////////////////////////////////////////////////////////////////////////
		protected override void CopyDataToInstance(in BTConditional InNewInstance)
		{
			var node = InNewInstance as BTConditional_BlackboardValue;
			node.m_BlackboardKey = m_BlackboardKey;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnableObserverInternal()
		{
			if (m_BlackboardKey)
			{
				m_Owner.Blackboard.AddObserver(m_BlackboardKey, OnValueChanged);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnRemoveObserverInternal()
		{
			if (m_BlackboardKey)
			{
				m_Owner.Blackboard.RemoveObserver(m_BlackboardKey, OnValueChanged);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool GetEvaluation()
		{
			if (m_BlackboardKey && m_Owner.Blackboard.TryGetEntryBase(m_BlackboardKey, out BlackboardEntryBase baseKey))
			{
				return baseKey.HasValue();
			}
			return false;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnValueChanged(bool bHasNewValue)
		{
			if (bHasNewValue && m_NodeConditionalized.AbortType == EAbortType.LowerPriority)
			{
				m_BehaviourTree.ConditionalFlowAbort(m_NodeConditionalized);
			}
		}
	}
}
