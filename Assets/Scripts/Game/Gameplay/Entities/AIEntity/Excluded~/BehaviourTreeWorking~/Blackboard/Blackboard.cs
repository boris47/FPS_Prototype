
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public sealed partial class Blackboard : MonoBehaviour
	{
		/// <summary> Stores the maximum abstracted blackboard entries </summary>
		[SerializeReference, ReadOnly]
		private List<BlackboardEntryBase> m_Entries = new List<BlackboardEntryBase>();

		private Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>> m_Observers =
			new Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>>();


		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntry<T>(BlackboardEntryKey InBlackboardKey, out T OutEntry, in bool bExactType = false) where T : BlackboardEntryBase, new()
		{
			OutEntry = default;
			if (TryGetEntryBase(InBlackboardKey, out BlackboardEntryBase OutValue))
			{
				if (ReflectionHelper.TryGetGenericArg(typeof(T), out System.Type storedGenericType))
				{
					if (bExactType)
					{
						if (OutValue.StoredType == storedGenericType)
						{
							OutEntry = (T)OutValue;
						}
					}
					else
					{
						if (ReflectionHelper.IsInerithedFrom(storedGenericType, OutValue.StoredType))
						{
							OutEntry = (T)OutValue;
						}
					}
				}
			}
			return OutEntry != default;
		}

		//////////////////////////////////////////////////////////////////////////
		private T GetOrCreateEntry<T>(in BlackboardEntryKey InBlackboardKey) where T : BlackboardEntryBase, new() => (T)GetOrCreateEntry(typeof(T), InBlackboardKey);

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryBase GetOrCreateEntry(System.Type type, in BlackboardEntryKey InBlackboardKey)
		{
			CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BlackboardEntryBase), type));
			BlackboardEntryBase OutValue = default;
			if (!TryGetEntryBase(InBlackboardKey, out OutValue))
			{
				EnsureDelegateListForBlackboardKey(InBlackboardKey, out var delegates);

				BlackboardEntryBase newInstance = (BlackboardEntryBase)System.Activator.CreateInstance(type);
				{
					newInstance.SetData(InBlackboardKey);
					newInstance.OnChangeNotification += OnChangeNotification;
				}
				OutValue = m_Entries.AddRef(newInstance);
			}
			return OutValue;
		}

		//////////////////////////////////////////////////////////////////////////
		private EOnChangeDelExecutionResult OnChangeNotification(in BlackboardEntryKey InBlackboardKey, in EBlackboardValueOp InOperation)
		{
			if (CustomAssertions.IsTrue(m_Observers.ContainsKey(InBlackboardKey)))
			{
				List<int> toRemove = new List<int>();

				var observersList = m_Observers[InBlackboardKey];
				for (int i = observersList.Count - 1; i >= 0; i--)
				{
					if (observersList[i](InBlackboardKey, InOperation) == EOnChangeDelExecutionResult.REMOVE)
					{
						toRemove.Add(i);
					}
				}
				foreach(int index in toRemove)
				{
					observersList.RemoveAt(index);
				}
			}
			return EOnChangeDelExecutionResult.LEAVE;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> If entry doesn't exist a new one is created and the new value set </summary>
		public BlackboardEntryBase SetEntryValue<T, V>(in BlackboardEntryKey InBlackboardKey, in V InNewValue) where T : BlackboardEntryKeyValue<V>, new()
		{
			T entry = GetOrCreateEntry<T>(InBlackboardKey);
			{
				entry.SetValue(InNewValue);
			}
			return entry;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveEntry(BlackboardEntryKey InBlackboardKey)
		{
			if (m_Entries.TryFind(out BlackboardEntryBase _, out int index, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey)))
			{
				List<int> toRemove = new List<int>();

				var observersList = m_Observers[InBlackboardKey];
				for (int i = observersList.Count - 1; i >= 0; i--)
				{
					if (observersList[i](InBlackboardKey, EBlackboardValueOp.REMOVE) == EOnChangeDelExecutionResult.REMOVE)
					{
						toRemove.Add(i);
					}
				}
				foreach (int indexx in toRemove)
				{
					observersList.RemoveAt(indexx);
				}

		//		m_Observers.Remove(InBlackboardKey);
				m_Entries.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			EnsureDelegateListForBlackboardKey(InBlackboardKey, out var delegates);
			delegates.Add(InObserverDelegate);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			if (m_Observers.ContainsKey(InBlackboardKey))
			{
				m_Observers[InBlackboardKey].Remove(InObserverDelegate);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void EnsureDelegateListForBlackboardKey(in BlackboardEntryKey InBlackboardKey, out List<BlackboardEntryBase.OnChangeDel> OutDelegates)
		{
			if (m_Observers.ContainsKey(InBlackboardKey))
			{
				OutDelegates = m_Observers[InBlackboardKey];
			}
			else
			{
				OutDelegates = new List<BlackboardEntryBase.OnChangeDel>();
				m_Observers.Add(InBlackboardKey, OutDelegates);
			}
		}
	}
}
