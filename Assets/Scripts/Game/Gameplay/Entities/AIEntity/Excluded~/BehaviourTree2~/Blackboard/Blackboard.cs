
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components
{
	[System.Serializable]
	public sealed class Blackboard : MonoBehaviour
	{
		/// <summary> Stores the maximum abstracted blackboard entries </summary>
		[SerializeReference, ReadOnly]
		private List<BlackboardEntryBase> m_Entries = new List<BlackboardEntryBase>();

		private Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>> m_Observers =
			new Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>>();

		public BlackboardEntryKey key;

		private void Awake()
		{
		//	SetEntryValue<BlackboardEntry_PositionToReach, Vector3>(key, Vector3.one);
		}
		public int count = 0;
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (TryGetEntryBase(key, out var _))
				{
					if (count < 2)
					{
						SetEntryValue<BlackboardEntry_PositionToReach, Vector3>(key, Vector3.one*Random.value);
						count++;
					}
					else
					{
						RemoveEntry(key);
						count = 0;
					}
				}
				else
				{
					SetEntryValue<BlackboardEntry_PositionToReach, Vector3>(key, Vector3.one);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public T GetEntry<T>(BlackboardEntryKey InBlackboardKey) where T : BlackboardEntryBase, new()
		{
			BlackboardEntryBase OutValue = default;
			if (!TryGetEntryBase(InBlackboardKey, out OutValue))
			{
				EnsureDelegateListForBlackboardKey(InBlackboardKey, out var delegates);

				BlackboardEntryBase newInstance = System.Activator.CreateInstance<T>();
				{
					newInstance.SetData(InBlackboardKey);
					newInstance.OnChangeNotification += OnChangeNotification;
				}
				OutValue = m_Entries.AddRef(newInstance);
			}
			return (T)OutValue;
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
			T entry = GetEntry<T>(InBlackboardKey);
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
