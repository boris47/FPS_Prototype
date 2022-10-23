using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public abstract class BlackboardEntryBase
	{
		public delegate void OnChangeDel(bool bhasValue);

		[SerializeField]
		private				BlackboardEntryKey		m_BlackboardEntryKey			= null;

		public				BlackboardEntryKey		BlackboardEntryKey				=> m_BlackboardEntryKey;

		public event		OnChangeDel				OnChangeNotification			= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public BlackboardEntryBase(in BlackboardEntryKey InBlackboardKey, in OnChangeDel InKeyObservers)
		{
			m_BlackboardEntryKey = InBlackboardKey;
			OnChangeNotification = InKeyObservers;
		}

		//////////////////////////////////////////////////////////////////////////
		public abstract bool HasValue();

		//////////////////////////////////////////////////////////////////////////
		protected void OnChangeNotificationInternal(bool bHasValue) => OnChangeNotification(bHasValue);
	}
}

