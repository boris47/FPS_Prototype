
namespace Entities.AI.Components
{
	using System.Collections.Generic;
	using UnityEngine;
	using Behaviours;

	//////////////////////////////////////////////////////////////////////////
	[System.Serializable]
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
		private List<BlackboardEntryBase>			m_Entries						= new List<BlackboardEntryBase>();

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
}