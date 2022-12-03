﻿using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public abstract class BlackboardEntryKeyValue<T> : BlackboardEntryBase
	{
		[SerializeField]
		private						T 						m_Value							= default;

		private	readonly			T						m_DefaultValue					= default;
		public						T						Value							=> m_Value;

		public sealed override		System.Type				StoredType						=> typeof(T);

		/// <summary> Called when value is changed (previous, new value) </summary>
		public event				System.Action<T, T>		OnValueChanged					= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public void SetValue(in T InNewValue)
		{
			bool bIsEqual = EqualityComparer<T>.Default.Equals(m_Value, InNewValue);
			if (!bIsEqual)
			{
				bool bNewValueIsDefault = EqualityComparer<T>.Default.Equals(InNewValue, m_DefaultValue);
				bool bCurrentValueIsDefault = !HasValue();

				// Change by default
				EBlackboardValueOp operation = EBlackboardValueOp.CHANGE;

				// Add value
				if (!bNewValueIsDefault && bCurrentValueIsDefault)
				{
					operation = EBlackboardValueOp.ADD;
				}
				// Remove
				else if (!bCurrentValueIsDefault && bNewValueIsDefault)
				{
					operation = EBlackboardValueOp.REMOVE;
				}

				T previousValue = m_Value;
				m_Value = InNewValue;
				OnValueChanged(previousValue, InNewValue);
				OnChangeNotificationInternal(BlackboardInstance, operation);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override sealed bool HasValue() => !EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);
	}
}

