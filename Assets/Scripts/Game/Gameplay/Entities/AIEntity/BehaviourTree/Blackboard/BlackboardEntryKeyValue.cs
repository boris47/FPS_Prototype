﻿using UnityEngine;

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
			bool bIsEqual = System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_Value, InNewValue);
			if (!bIsEqual)
			{
				EBlackboardValueOp OutOperation = EBlackboardValueOp.CHANGE;
				bool bIsUnassigned = System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);
				if (bIsUnassigned)
				{
					OutOperation = EBlackboardValueOp.ADD;
				}
				else
				{
					bool bIsReset = System.Collections.Generic.EqualityComparer<T>.Default.Equals(InNewValue, m_DefaultValue);
					if (bIsReset)
					{
						OutOperation = EBlackboardValueOp.REMOVE;
					}
				}

				OnValueChanged(Value, InNewValue);
				OnChangeNotificationInternal(OutOperation);
			}
			m_Value = InNewValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool HasValue() => !System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_Value, m_DefaultValue);
	}
}

