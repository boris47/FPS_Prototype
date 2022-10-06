
namespace Entities.AI.Components
{
	using System.Collections.Generic;
	using UnityEngine;

	[System.Serializable]
	public sealed partial class Blackboard : ScriptableObject
	{
		[SerializeField, ReadOnly]
		private Blackboard m_Parent = null;

		[SerializeField, ReadOnly]
		private List<BlackboardKeySpecifier> m_Keys = new List<BlackboardKeySpecifier>();

		[SerializeReference, ReadOnly]
		private List<BlackboardEntryBase> m_Entries = new List<BlackboardEntryBase>();

		private Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>> m_Observers =
			new Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>>();



		//////////////////////////////////////////////////////////////////////////
		public bool HasKeyRegistered(BlackboardEntryKey InBlackboardKey)
		{
			return m_Keys.Exists(BBKey => BBKey.Key.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public bool AddKey(BlackboardEntryKey InBlackboardKey, in System.Type InSupportedType)
		{
			bool outValue = false;
			if (!m_Keys.TryFind(out BlackboardKeySpecifier _, out int _, BBKey => BBKey.Key.IsEqualTo(InBlackboardKey)))
			{
				m_Keys.Add(BlackboardKeySpecifier.Create(InBlackboardKey, InSupportedType));
				outValue = true;
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool RemoveKey(BlackboardEntryKey InBlackboardKey)
		{
			bool outValue = false;
			if (m_Keys.TryFind(out BlackboardKeySpecifier _, out int index, BBKey => BBKey.Key.IsEqualTo(InBlackboardKey)))
			{
				m_Keys.RemoveAt(index);
				outValue = true;
			}
			return outValue;
		}







		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntry<T>(BlackboardEntryKey InBlackboardKey, out T OutEntry) where T : BlackboardEntryBase, new()
		{
			OutEntry = default;

			bool OutResult = TryGetEntry(typeof(T), InBlackboardKey, out BlackboardEntryBase OutBaseEntry);
			if (OutResult)
			{
				OutEntry = OutBaseEntry as T;
			}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private bool TryGetEntry(in System.Type InEntryType, in BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			OutEntry = null;
			if (TryGetEntryBase(InBlackboardKey, out BlackboardEntryBase foundEntry))
			{
				if (Utils.CustomAssertions.IsNotNull(foundEntry.StoredType))
				{
					// If same key type
					if (InEntryType == foundEntry.GetType())
					{
						if (ReflectionHelper.TryGetGenericArg(InEntryType, out System.Type storedGenericType))
						{
							// If stored type is equal or derived of requested type
							if (foundEntry.StoredType == storedGenericType || ReflectionHelper.IsInerithedFrom(storedGenericType, foundEntry.StoredType))
							{
								OutEntry = foundEntry;
							}
						}
					}
				}
			}
			return OutEntry.IsNotNull();
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <returns>true for equal type, false for not equal type, null if not assigned</returns>
		public bool? HasEntryOfType(in BlackboardEntryKey InBlackboardKey, in System.Type InEntryType)
		{
			bool? OutResult = null;
			if (TryGetEntryBase(InBlackboardKey, out BlackboardEntryBase foundEntry))
			{
				OutResult = InEntryType == foundEntry.GetType();
			}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private T GetOrCreateEntry<T>(in BlackboardEntryKey InBlackboardKey) where T : BlackboardEntryBase, new() => (T)GetOrCreateEntry(typeof(T), InBlackboardKey);

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryBase GetOrCreateEntry(in System.Type InBlackboardEntryKeyValueType, in BlackboardEntryKey InBlackboardKey)
		{
			Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BlackboardEntryBase), InBlackboardEntryKeyValueType));
			BlackboardEntryBase OutValue = default;
			if (!TryGetEntry(InBlackboardEntryKeyValueType, InBlackboardKey, out OutValue))
			{
				EnsureDelegateListForBlackboardKey(InBlackboardKey);

				BlackboardEntryBase newInstance = (BlackboardEntryBase)System.Activator.CreateInstance(InBlackboardEntryKeyValueType);
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
			if (Utils.CustomAssertions.IsTrue(m_Observers.ContainsKey(InBlackboardKey)))
			{
				List<int> toRemove = new List<int>();

				List<BlackboardEntryBase.OnChangeDel> observersList = m_Observers[InBlackboardKey];
				for (int i = observersList.Count - 1; i >= 0; i--)
				{
					if (observersList[i](InBlackboardKey, InOperation) == EOnChangeDelExecutionResult.REMOVE)
					{
						toRemove.Add(i);
					}
				}
				foreach (int index in toRemove)
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

				List<BlackboardEntryBase.OnChangeDel> observersList = m_Observers[InBlackboardKey];
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

				m_Keys.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			EnsureDelegateListForBlackboardKey(InBlackboardKey).Add(InObserverDelegate);
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
		private List<BlackboardEntryBase.OnChangeDel> EnsureDelegateListForBlackboardKey(in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> OutDelegates = null;
			if (m_Observers.ContainsKey(InBlackboardKey))
			{
				OutDelegates = m_Observers[InBlackboardKey];
			}
			else
			{
				OutDelegates = new List<BlackboardEntryBase.OnChangeDel>();
				m_Observers.Add(InBlackboardKey, OutDelegates);
			}
			return OutDelegates;
		}


	}
}