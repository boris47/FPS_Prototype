using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public abstract class BlackboardEntryKeyValue<T> : BlackboardEntryBase
	{
		[SerializeField]
		private			T 						m_Value			= default;

		private			T						m_DefaultValue	= default;
		public			T						Value			=> m_Value;

		public sealed override	System.Type		StoredType		=> typeof(T);

		/// <summary> Called when value is changed (previous, new value) </summary>
		public event	System.Action<T, T>		OnValueChanged	= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public void SetValue(in T InNewValue)
		{
			bool bIsEqual = EqualityComparer<T>.Default.Equals(m_Value, InNewValue);
			if (!bIsEqual)
			{
				// Change by default
				EBlackboardValueOp OutOperation = EBlackboardValueOp.CHANGE;

				bool bNewValueIsDefault = EqualityComparer<T>.Default.Equals(InNewValue, m_DefaultValue);
				bool bCurrentValueIsDefault = EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);

				// Add value
				if (!bNewValueIsDefault && bCurrentValueIsDefault)
				{
					OutOperation = EBlackboardValueOp.ADD;
				}
				// Remove
				else if (!bCurrentValueIsDefault && bNewValueIsDefault)
				{
					OutOperation = EBlackboardValueOp.REMOVE;
				}

				OnValueChanged(m_Value, InNewValue);
				OnChangeNotificationInternal(OutOperation);

				m_Value = InNewValue;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool HasValue() => !EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);
	}
}

