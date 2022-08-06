using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	public enum EBlackboardValueOp
	{
		ADD, CHANGE, REMOVE
	}

	public enum EOnChangeDelExecutionResult
	{
		LEAVE, REMOVE
	}

	[System.Serializable]
	public abstract class BlackboardEntryBase
	{
		public delegate EOnChangeDelExecutionResult OnChangeDel(in BlackboardEntryKey InBlackboardKey, in EBlackboardValueOp InOperation);

		[SerializeField]
		private				BlackboardEntryKey		m_BlackboardEntryKey			= null;

		public event		OnChangeDel				OnChangeNotification			= delegate { return EOnChangeDelExecutionResult.LEAVE; };
		public				BlackboardEntryKey		BlackboardEntryKey				=> m_BlackboardEntryKey;

		public abstract		System.Type				StoredType						{ get; }

		public void SetData(in BlackboardEntryKey InBlackboardKey)
		{
			m_BlackboardEntryKey = InBlackboardKey;
		}

		//////////////////////////////////////////////////////////////////////////
		public abstract bool HasValue();

		//////////////////////////////////////////////////////////////////////////
		protected void OnChangeNotificationInternal(in EBlackboardValueOp InOperation) => OnChangeNotification(m_BlackboardEntryKey, InOperation);
	}
}

