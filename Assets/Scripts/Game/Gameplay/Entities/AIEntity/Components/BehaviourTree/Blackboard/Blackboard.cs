
namespace Entities.AI.Components
{
	using System.Collections.Generic;
	using UnityEngine;
	using Behaviours;


	//////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public sealed partial class Blackboard : ScriptableObject
	{
		[SerializeField, HideInInspector]
		private List<BlackboardKeySpecifier> m_Keys = new List<BlackboardKeySpecifier>(); // Key - Value Type

		//////////////////////////////////////////////////////////////////////////
		private void OnEnable()
		{
			// Load Types
			m_Keys.ForEach(k => k.Load());
		}

		//////////////////////////////////////////////////////////////////////////
		public static BlackboardInstanceData CreateInstanceData(in Blackboard InBlackboardAsset, in BehaviourTreeInstanceData InTreeInstanceData)
		{
			Blackboard blackboardInstance = CreateInstance<Blackboard>();
			BlackboardInstanceData blackboardInstanceData = new BlackboardInstanceData(InTreeInstanceData, InBlackboardAsset, blackboardInstance);
			return blackboardInstanceData;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(in BlackboardInstanceData InBlackboardInstanceData, BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return InBlackboardInstanceData.Entries.TryFind(out OutEntry, out int _, e => e.BlackboardEntryKey == InBlackboardKey);
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
				InBlackboardInstanceData.Entries.Add(OutEntry);
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
		public void SetEntryValue<T, V>(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in V InNewValue) where T : BlackboardEntryKeyValue<V>, new()
		{
			if (InBlackboardKey.IsValid())
			{
				T entry = GetOrCreateEntry<T>(InBlackboardInstanceData, InBlackboardKey);
				{
					entry.SetValue(InNewValue);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveEntry(in BlackboardInstanceData InBlackboardInstanceData, BlackboardEntryKey InBlackboardKey)
		{
			if (InBlackboardKey.IsValid() && InBlackboardInstanceData.TryGetIndexOfEntry(InBlackboardKey, out int index))
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

				InBlackboardInstanceData.Entries.RemoveAt(index);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void AddObserver(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			EnsureDelegateListForBlackboardKey(InBlackboardInstanceData, InBlackboardKey).AddUnique(InObserverDelegate);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserver(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey, in BlackboardEntryBase.OnChangeDel InObserverDelegate)
		{
			if (InBlackboardInstanceData.AreThereObserversForKey(InBlackboardKey))
			{
				InBlackboardInstanceData.GetObserversFor(InBlackboardKey)?.Remove(InObserverDelegate);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private List<BlackboardEntryBase.OnChangeDel> EnsureDelegateListForBlackboardKey(in BlackboardInstanceData InBlackboardInstanceData, in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> OutDelegates = null;
			if (InBlackboardKey.IsValid())
			{
				if (InBlackboardInstanceData.AreThereObserversForKey(InBlackboardKey))
				{
					OutDelegates = InBlackboardInstanceData.GetObserversFor(InBlackboardKey);
				}
				else
				{
					OutDelegates = InBlackboardInstanceData.CreateObserversFor(InBlackboardKey);
				}
			}
			return OutDelegates;
		}

#if UNITY_EDITOR
		public static class Editor
		{
			public static event System.Action OnKeysModified = delegate { };

			public static string KeyListPropertyName => nameof(Blackboard.m_Keys);

			//////////////////////////////////////////////////////////////////////////
			public static void AddKey(in Blackboard InBlackboard, BlackboardEntryKey InBlackboardKey, in System.Type InSupportedType)
			{
				static int Comparer(BlackboardKeySpecifier x, BlackboardKeySpecifier y) => string.Compare(x.Key.Name, y.Key.Name);

				InBlackboard.m_Keys.Add(BlackboardKeySpecifier.Editor.Create(InBlackboardKey, InSupportedType));

				InBlackboard.m_Keys.Sort(Comparer);

				OnKeysModified();
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool HasKey(in Blackboard InBlackboard, BlackboardEntryKey InBlackboardKey)
			{
				return InBlackboard.m_Keys.Exists(BBKey => BBKey.Key == InBlackboardKey);
			}

			//////////////////////////////////////////////////////////////////////////
			public static void RenameKey(in BlackboardKeySpecifier InKeySpecifier, in string InNewName)
			{
				BlackboardKeySpecifier.Editor.Rename(InKeySpecifier, InNewName);
				OnKeysModified();
			}

			//////////////////////////////////////////////////////////////////////////
			public static void ChangeTypeForKey(in BlackboardKeySpecifier InKeySpecifier, in System.Type InNewType)
			{
				BlackboardKeySpecifier.Editor.SetType(InKeySpecifier, InNewType);
				OnKeysModified();
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetKey(in Blackboard InBlackboard, BlackboardEntryKey InBlackboardKey, out BlackboardKeySpecifier OutSpecifier)
			{
				return InBlackboard.m_Keys.TryFind(out OutSpecifier, out int _, keySpecifier => keySpecifier.Key == InBlackboardKey);
			}

			//////////////////////////////////////////////////////////////////////////
			public static bool TryGetKeyAt(in Blackboard InBlackboard, int InIndex, out BlackboardKeySpecifier OutSpecifier)
			{
				return InBlackboard.m_Keys.TryGetByIndex(InIndex, out OutSpecifier);
			}

			//////////////////////////////////////////////////////////////////////////
			public static int GetKeysCount(in Blackboard InBlackboard) => InBlackboard.m_Keys.Count;

			//////////////////////////////////////////////////////////////////////////
			public static bool RemoveKey(in Blackboard InBlackboard, BlackboardEntryKey InBlackboardKey)
			{
				bool outValue = false;
				if (InBlackboard.m_Keys.TryFind(out BlackboardKeySpecifier _, out int index, BBKey => BBKey.Key == InBlackboardKey))
				{
					InBlackboard.m_Keys.RemoveAt(index);
					outValue = true;
					OnKeysModified();
				}
				return outValue;
			}

			//////////////////////////////////////////////////////////////////////////
			public static void EnsureKeysLoaded(in Blackboard InBlackboard)
			{
				InBlackboard.m_Keys.ForEach(k => k.Load());
			}
		}
#endif
	}
}