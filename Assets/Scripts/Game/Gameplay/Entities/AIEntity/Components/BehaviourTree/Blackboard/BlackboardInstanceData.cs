
namespace Entities.AI.Components
{
	using System.Collections.Generic;
	using UnityEngine;
	using Behaviours;

	//////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public class BlackboardInstanceData
	{
		private	readonly	Blackboard						m_BlackboardAsset				= null;
	//	private readonly	Blackboard						m_BlackboardInstance			= null;
		private readonly	BehaviourTreeInstanceData		m_BehaviourTreeInstanceData		= null;

		/// <summary> Key - EntryValue<T> </summary>
		[SerializeReference, ReadOnly]
		private List<BlackboardEntryBase>			m_Entries						= new List<BlackboardEntryBase>();

		/// <summary> Key ID - Observers </summary>
		private Dictionary<uint, List<BlackboardEntryBase.OnChangeDel>> m_Observers = 
			new Dictionary<uint, List<BlackboardEntryBase.OnChangeDel>>();


		public		Blackboard						BlackboardAsset					=> m_BlackboardAsset;
	//	public		Blackboard						BlackboardInstance				=> m_BlackboardInstance;
		public		BehaviourTreeInstanceData		BehaviourTreeInstanceData		=> m_BehaviourTreeInstanceData;
		public		List<BlackboardEntryBase>		Entries							=> m_Entries;

		public event System.Action					OnEntriesUpdate					= delegate { };


		//////////////////////////////////////////////////////////////////////////
		public BlackboardInstanceData(in BehaviourTreeInstanceData InBehaviourTreeInstanceData, in Blackboard InBlackboardAsset, in Blackboard InBlackboardInstance)
		{
			m_BehaviourTreeInstanceData = InBehaviourTreeInstanceData;
			m_BlackboardAsset = InBlackboardAsset;
		//	m_BlackboardInstance = InBlackboardInstance;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntryBase(BlackboardEntryKey InBlackboardKey, out BlackboardEntryBase OutEntry)
		{
			return m_BlackboardAsset.TryGetEntryBase(this, InBlackboardKey, out OutEntry);
		}

		//////////////////////////////////////////////////////////////////////////
		public void SetEntryValue<T, V>(in BlackboardEntryKey InBlackboardKey, in V InNewValue) where T : BlackboardEntryKeyValue<V>, new()
		{
			m_BlackboardAsset.SetEntryValue<T, V>(this, InBlackboardKey, InNewValue);
			OnEntriesUpdate();
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetEntry<T>(BlackboardEntryKey InBlackboardKey, out T OutEntry) where T : BlackboardEntryBase, new()
		{
			return m_BlackboardAsset.TryGetEntry(this, InBlackboardKey, out OutEntry);
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveEntry(BlackboardEntryKey InBlackboardKey)
		{
			m_BlackboardAsset.RemoveEntry(this, InBlackboardKey);
			OnEntriesUpdate();
		}

		private EOnChangeDelExecutionResult Updater(in BlackboardInstanceData InBlackboardInstance, in BlackboardEntryKey InBlackboardKey, in EBlackboardValueOp InOperation)
		{
			OnEntriesUpdate();
			return EOnChangeDelExecutionResult.LEAVE;
		}

		//////////////////////////////////////////////////////////////////////////
		public List<BlackboardEntryBase.OnChangeDel> CreateObserversFor(in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> OutDelegates = new List<BlackboardEntryBase.OnChangeDel>();
			if (InBlackboardKey.IsValid())
			{
				OutDelegates.Add(Updater);
				m_Observers.Add(InBlackboardKey.UniqueId, OutDelegates);
			}
			return OutDelegates;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RemoveObserversFor(in BlackboardEntryKey InBlackboardKey)
		{
			if (InBlackboardKey.IsValid())
			{
				m_Observers.Remove(InBlackboardKey.UniqueId);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool AreThereObserversForKey(in BlackboardEntryKey InBlackboardKey) => InBlackboardKey.IsValid() && m_Observers.ContainsKey(InBlackboardKey.UniqueId);

		//////////////////////////////////////////////////////////////////////////
		public List<BlackboardEntryBase.OnChangeDel> GetObserversFor(in BlackboardEntryKey InBlackboardKey)
		{
			List<BlackboardEntryBase.OnChangeDel> outValue = null;
			if (InBlackboardKey.IsValid() && !m_Observers.TryGetValue(InBlackboardKey.UniqueId, out outValue))
			{
				outValue = new List<BlackboardEntryBase.OnChangeDel>();
			}
			return outValue;
		}

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetIndexOfEntry(BlackboardEntryKey InBlackboardKey, out int OutIndex)
		{
			return m_Entries.TryFind(out BlackboardEntryBase _, out OutIndex, e => e.BlackboardEntryKey == InBlackboardKey);
		}
	}
}