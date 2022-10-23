using UnityEngine;

namespace Entities.AI
{
	using System.Collections.Generic;
	using System.Linq;
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
		private BlackboardInstanceData m_BlackboardInstance = null;

		[SerializeField]
		private BlackboardEntryKey m_BlackboardEntryKey = null;

		private OnChangeDel m_OnChangeNotification = delegate { return EOnChangeDelExecutionResult.LEAVE; };

		public BlackboardInstanceData BlackboardInstance => m_BlackboardInstance;
		public BlackboardEntryKey BlackboardEntryKey => m_BlackboardEntryKey;

		public abstract System.Type StoredType { get; }

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

#if UNITY_EDITOR
		/*
		Le derivate di BlackboardEntryKey, in base al tipo del loro valore vanno a determinare i tipi che a loro volta possono essere referenziati dalle chiavi
		nella blackboard.

		Azione:
			Da tutti i derivati di BlackboardEntryKey creare mappa [entry type - entry value type] e creare un set di tutti i valori delle entries
			Grazie a questo set si sa ora di che tipo possone essere create le chiavi della blackboard.
			Nel momento in cui un tipo di una chiave della blackboard non viene risolto, bisogna resettare ogni suo precedente riferimento

		*/


		public static class Editor
		{
			private static readonly Dictionary<System.Type, System.Type> s_MappedTypes = new Dictionary<System.Type, System.Type>();
			private static readonly HashSet<System.Type> s_EntryValueTypes = new HashSet<System.Type>();

			[UnityEditor.Callbacks.DidReloadScripts]
			public static void AfterLoad()
			{
				foreach (System.Type entryConcreteType in UnityEditor.TypeCache.GetTypesDerivedFrom<BlackboardEntryBase>().Where(t => !t.IsAbstract))
				{
					if (Utils.CustomAssertions.IsTrue(ReflectionHelper.TryGetGenericArg(entryConcreteType, out System.Type OutEntryValueType)))
					{
						s_MappedTypes.Add(entryConcreteType, OutEntryValueType);
						s_EntryValueTypes.Add(OutEntryValueType);
					}
				}
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetEntryTypeForEntryOfType(in System.Type InEntryType, out System.Type OutEntryValueType)
			{
				return s_MappedTypes.TryGetValue(InEntryType, out OutEntryValueType);
			}

			//////////////////////////////////////////////////////////////////////////
			public static System.Type[] GetEntryValueTypes() => s_EntryValueTypes.ToArray();
		}
#endif
	}
}

