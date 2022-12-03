using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	[System.Serializable]
	public sealed class AIBehaviourBlackboardEntryKeyValue<T> : AIBehaviourBlackboardEntry
	{
		public delegate void OnValueChangedDel(EAIBehaviourBlackboardValueOp InOperation, in T InPreviousValue, in T InNewValue);

		[SerializeField]
		private						T 						m_Value							= default;

		private	readonly			T						m_DefaultValue					= default;
		public						T						Value							=> m_Value;

		public sealed override		System.Type				StoredType						=> typeof(T);

		/// <summary> Called when value is changed (operation, previous value, new value) </summary>
		public event				OnValueChangedDel		OnValueChanged					= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public void SetValue(in T InNewValue)
		{
			bool bIsEqual = EqualityComparer<T>.Default.Equals(m_Value, InNewValue);
			if (!bIsEqual)
			{
				bool bNewValueIsDefault = EqualityComparer<T>.Default.Equals(InNewValue, m_DefaultValue);
				bool bCurrentValueIsDefault = !HasValue();

				// Change by default
				EAIBehaviourBlackboardValueOp operation = EAIBehaviourBlackboardValueOp.CHANGE;
				
				// Add value
				if (!bNewValueIsDefault && bCurrentValueIsDefault)
				{
					operation = EAIBehaviourBlackboardValueOp.ADD;
				}
				// Remove
				else if (!bCurrentValueIsDefault && bNewValueIsDefault)
				{
					operation = EAIBehaviourBlackboardValueOp.REMOVE;
				}

				T previousValue = m_Value;
				m_Value = InNewValue;
				OnValueChanged(operation, previousValue, InNewValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override sealed bool HasValue() => !EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);
	}
}

