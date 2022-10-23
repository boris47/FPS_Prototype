using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public abstract class BlackboardEntryKeyValue<T> : BlackboardEntryBase
	{
		private			T						m_Value			= default;
		public			T						Value			=> m_Value;

		public event	System.Action<T, T>		OnValueChanged	= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public BlackboardEntryKeyValue(in BlackboardEntryKey InBlackboardKey, in OnChangeDel InKeyObservers) : base(InBlackboardKey, InKeyObservers)
		{

		}

		//////////////////////////////////////////////////////////////////////////
		public void SetValue(in T InNewValue)
		{
			m_Value = InNewValue;
			OnValueChanged(Value, InNewValue);
			OnChangeNotificationInternal(HasValue());
		}

		//////////////////////////////////////////////////////////////////////////
		public override bool HasValue() => !System.Collections.Generic.EqualityComparer<T>.Default.Equals(m_Value, default(T));
	}
}

