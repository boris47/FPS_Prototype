using UnityEngine;

namespace Entities.AI
{
	using Components;

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
		public delegate EOnChangeDelExecutionResult OnChangeDel(in BlackboardInstanceData InBlackboardInstance, in BlackboardEntryKey InBlackboardKey, in EBlackboardValueOp InOperation);

		[SerializeField]
		private				BlackboardInstanceData			m_BlackboardInstance			= null;

		[SerializeField]
		private				BlackboardEntryKey				m_BlackboardEntryKey			= null;

		private				OnChangeDel						m_OnChangeNotification			= delegate { return EOnChangeDelExecutionResult.LEAVE; };

		public				BlackboardInstanceData			BlackboardInstance				=> m_BlackboardInstance;
		public				BlackboardEntryKey				BlackboardEntryKey				=> m_BlackboardEntryKey;

		public abstract		System.Type						StoredType						{ get; }

		public void SetData(in BlackboardInstanceData InBlackboardInstance, in BlackboardEntryKey InBlackboardKey, in OnChangeDel InOnChange)
		{
			m_BlackboardInstance = InBlackboardInstance;
			m_BlackboardEntryKey = InBlackboardKey;
			m_OnChangeNotification = InOnChange;
		}

		//////////////////////////////////////////////////////////////////////////
		public abstract bool HasValue();

		//////////////////////////////////////////////////////////////////////////
		protected void OnChangeNotificationInternal(in BlackboardInstanceData InBlackboardInstance, in EBlackboardValueOp InOperation)
		{
			m_OnChangeNotification(InBlackboardInstance, m_BlackboardEntryKey, InOperation);
		}
	}
}

