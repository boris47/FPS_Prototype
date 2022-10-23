
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

	//	[SerializeField, UDictionary.ReadOnly]
		private Dictionary<BlackboardEntryKey, BlackboardEntryBase.OnChangeDel> m_Observers = new Dictionary<BlackboardEntryKey, BlackboardEntryBase.OnChangeDel>();

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public T GetEntry<T>(in BlackboardEntryKey InBlackboardKey) where T : BlackboardEntryBase, new()
		{
			BlackboardEntryBase OutValue = default;
			if (!TryGetEntryBase(InBlackboardKey, out OutValue))
			{
				BlackboardEntryBase.OnChangeDel keyObservers = CreateOrKeyGetObserverDelegates(InBlackboardKey);
				var args = new object[] { InBlackboardKey, keyObservers };
				BlackboardEntryBase newInstance = (BlackboardEntryBase)System.Activator.CreateInstance(typeof(T), args);
				OutValue = m_Entries.AddRef(newInstance);
			}
			return (T)OutValue;
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
				m_Observers.Remove(InBlackboardKey);
				m_Entries.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			BlackboardEntryBase.OnChangeDel keyObservers = CreateOrKeyGetObserverDelegates(InBlackboardKey);
			keyObservers += InObserverDelegate;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			if (m_Observers.TryGetValue(InBlackboardKey, out BlackboardEntryBase.OnChangeDel OutDelegate))
			{
				OutDelegate -= InObserverDelegate;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryBase.OnChangeDel CreateOrKeyGetObserverDelegates(in BlackboardEntryKey InBlackboardKey)
		{
			BlackboardEntryBase.OnChangeDel OutDelegate;
			if (!m_Observers.TryGetValue(InBlackboardKey, out OutDelegate))
			{
				OutDelegate = delegate { };
				m_Observers.Add(InBlackboardKey, OutDelegate);
			}
			return OutDelegate;
		}
	}
}
