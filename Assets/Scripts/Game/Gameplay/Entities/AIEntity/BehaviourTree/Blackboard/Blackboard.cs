
namespace Entities.AI.Components
{
	using System.Collections.Generic;
	using UnityEngine;
	using Behaviours;

	//////////////////////////////////////////////////////////////////////////
	public class BlackboardInstanceData
	{
		[SerializeField, ReadOnly]
		private		Blackboard						m_BlackboardAsset				= null;

		[SerializeField, ReadOnly]
		private		Blackboard						m_BlackboardInstance			= null;

		[SerializeField, ReadOnly]
		private		BehaviourTreeInstanceData		m_BehaviourTreeInstanceData		= null;

		/// <summary> Key - EntryValue<T> </summary>
		[SerializeReference, ReadOnly]
		private List<BlackboardEntryBase> m_Entries = new List<BlackboardEntryBase>();

		/// <summary> Key - Observers </summary>
		private Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>> m_Observers = 
			new Dictionary<BlackboardEntryKey, List<BlackboardEntryBase.OnChangeDel>>();


		public		Blackboard						BlackboardAsset					=> m_BlackboardAsset;
		public		Blackboard						BlackboardInstance				=> m_BlackboardInstance;
 		public		BehaviourTreeInstanceData		BehaviourTreeInstanceData		=> m_BehaviourTreeInstanceData;
		public		List<BlackboardEntryBase>		Entries							=> m_Entries;


		//////////////////////////////////////////////////////////////////////////
		public BlackboardInstanceData(in BehaviourTreeInstanceData InBehaviourTreeInstanceData, in Blackboard InBlackboardAsset, in Blackboard InBlackboardInstance)
		{
			m_BehaviourTreeInstanceData = InBehaviourTreeInstanceData;
			m_BlackboardAsset = InBlackboardAsset;
			m_BlackboardInstance = InBlackboardInstance;
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddEntry(in BlackboardEntryBase InEntry)
		{
			m_Entries.Add(InEntry);
		}

		//////////////////////////////////////////////////////////////////////////
		public List<BlackboardEntryBase.OnChangeDel> CreateObserversFor(in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> OutDelegates = new List<BlackboardEntryBase.OnChangeDel>();
			m_Observers.Add(InBlackboardKey, OutDelegates);
			return OutDelegates;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserversFor(in BlackboardEntryKey InBlackboardKey)
		{
			m_Observers.Remove(InBlackboardKey);
		}

		//////////////////////////////////////////////////////////////////////////
		public bool AreThereObserversForKey(in BlackboardEntryKey InBlackboardKey) => m_Observers.ContainsKey(InBlackboardKey);

		//////////////////////////////////////////////////////////////////////////
		public List<BlackboardEntryBase.OnChangeDel> GetObserversFor(in BlackboardEntryKey InBlackboardKey) => m_Observers[InBlackboardKey];

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetIndexOfEntry(BlackboardEntryKey InBlackboardKey, out int OutIndex)
		{
			return m_Entries.TryFind(out BlackboardEntryBase _, out OutIndex, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public sealed partial class Blackboard : ScriptableObject
	{
		[SerializeField, ReadOnly]
		private List<BlackboardKeySpecifier> m_Keys = new List<BlackboardKeySpecifier>(); // Key - Value Type


		//////////////////////////////////////////////////////////////////////////
		public static BlackboardInstanceData CreateInstanceData(in Blackboard InBlackboardAsset, in BehaviourTreeInstanceData InTreeInstanceData)
		{
			Blackboard blackboardInstance = CreateInstance<Blackboard>();
			BlackboardInstanceData blackboardInstanceData = new BlackboardInstanceData(InTreeInstanceData, InBlackboardAsset, blackboardInstance);
			return blackboardInstanceData;
		}

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
		public bool TryGetEntryBase(in BlackboardInstanceData InBlackboardInstanceData, BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return InBlackboardInstanceData.Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey.IsEqualTo(InBlackboardKey));
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntry<T>(in BlackboardInstanceData InBlackboardInstanceData, BlackboardEntryKey InBlackboardKey, out T OutEntry) where T : BlackboardEntryBase, new()
		{
			OutEntry = default;

			bool OutResult = TryGetEntry(InBlackboardInstanceData, typeof(T), InBlackboardKey, out BlackboardEntryBase OutBaseEntry);
			if (OutResult)
			{
				OutEntry = OutBaseEntry as T;
			}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private bool TryGetEntry(in BlackboardInstanceData InBlackboardInstanceData, in System.Type InEntryType, in BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			OutEntry = null;
			if (TryGetEntryBase(InBlackboardInstanceData, InBlackboardKey, out BlackboardEntryBase foundEntry))
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
		public bool? HasEntryOfType(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in System.Type InEntryType)
		{
			bool? OutResult = null;
			if (TryGetEntryBase(InBlackboardInstanceData, InBlackboardKey, out BlackboardEntryBase foundEntry))
			{
				OutResult = InEntryType == foundEntry.GetType();
			}
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		private T GetOrCreateEntry<T>(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey) where T : BlackboardEntryBase, new()
		{
			return GetOrCreateEntry(InBlackboardInstanceData, typeof(T), InBlackboardKey) as T;
		}

		//////////////////////////////////////////////////////////////////////////
		private BlackboardEntryBase GetOrCreateEntry(in BlackboardInstanceData InBlackboardInstanceData, in System.Type InBlackboardEntryKeyValueType, in BlackboardEntryKey InBlackboardKey)
		{
			Utils.CustomAssertions.IsTrue(ReflectionHelper.IsInerithedFrom(typeof(BlackboardEntryBase), InBlackboardEntryKeyValueType));

			BlackboardEntryBase OutEntry = default;
			if (!TryGetEntry(InBlackboardInstanceData, InBlackboardEntryKeyValueType, InBlackboardKey, out OutEntry))
			{
				EnsureDelegateListForBlackboardKey(InBlackboardInstanceData, InBlackboardKey);

				OutEntry = (BlackboardEntryBase)System.Activator.CreateInstance(InBlackboardEntryKeyValueType);
				{
					OutEntry.SetData(InBlackboardInstanceData, InBlackboardKey, OnChangeNotification);
				}
				InBlackboardInstanceData.AddEntry(OutEntry);
			}
			return OutEntry;
		}

		//////////////////////////////////////////////////////////////////////////
		private EOnChangeDelExecutionResult OnChangeNotification(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in EBlackboardValueOp InOperation)
		{
			if (Utils.CustomAssertions.IsTrue(InBlackboardInstanceData.AreThereObserversForKey(InBlackboardKey)))
			{
				List<int> toRemove = new List<int>();

				List<BlackboardEntryBase.OnChangeDel> observersList = InBlackboardInstanceData.GetObserversFor(InBlackboardKey);
				for (int i = observersList.Count - 1; i >= 0; i--)
				{
					if (observersList[i](InBlackboardInstanceData, InBlackboardKey, InOperation) == EOnChangeDelExecutionResult.REMOVE)
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
		public BlackboardEntryBase SetEntryValue<T, V>(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in V InNewValue) where T : BlackboardEntryKeyValue<V>, new()
		{
			T entry = GetOrCreateEntry<T>(InBlackboardInstanceData, InBlackboardKey);
			{
				entry.SetValue(InNewValue);
			}
			return entry;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveEntry(in BlackboardInstanceData InBlackboardInstanceData, BlackboardEntryKey InBlackboardKey)
		{
			if (InBlackboardInstanceData.TryGetIndexOfEntry(InBlackboardKey, out int index))
			{
				List<int> toRemove = new List<int>();

				List<BlackboardEntryBase.OnChangeDel> observersList = InBlackboardInstanceData.GetObserversFor(InBlackboardKey);
				for (int i = observersList.Count - 1; i >= 0; i--)
				{
					if (observersList[i](InBlackboardInstanceData, InBlackboardKey, EBlackboardValueOp.REMOVE) == EOnChangeDelExecutionResult.REMOVE)
					{
						toRemove.Add(i);
					}
				}
				foreach (int indexx in toRemove)
				{
					observersList.RemoveAt(indexx);
				}

				if (observersList.Count == 0)
				{
					InBlackboardInstanceData.RemoveObserversFor(InBlackboardKey);
				}

				m_Keys.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			EnsureDelegateListForBlackboardKey(InBlackboardInstanceData, InBlackboardKey).Add(InObserverDelegate);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserver(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			if (InBlackboardInstanceData.AreThereObserversForKey(InBlackboardKey))
			{
				InBlackboardInstanceData.GetObserversFor(InBlackboardKey).Remove(InObserverDelegate);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private List<BlackboardEntryBase.OnChangeDel> EnsureDelegateListForBlackboardKey(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> OutDelegates = null;
			if (InBlackboardInstanceData.AreThereObserversForKey(InBlackboardKey))
			{
				OutDelegates = InBlackboardInstanceData.GetObserversFor(InBlackboardKey);
			}
			else
			{
				OutDelegates = InBlackboardInstanceData.CreateObserversFor(InBlackboardKey);
			}
			return OutDelegates;
		}


	}
}