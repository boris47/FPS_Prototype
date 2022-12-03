using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI.Components.Behaviours
{
	public class AIBehaviourBlackboard : ScriptableObject
	{
		private				List<AIBehaviourBlackboardEntry>				m_Entries										= new List<AIBehaviourBlackboardEntry>();

		public static class Keys
		{
			public const string kCurrentEntityTarget = "CurrentEntityTarget";
			public const string kCurrentLastEntityTarget = "CurrentLastEntityTarget";
		}

		/*
		public void SetEntry(string InBlackboardKey, in AIBehaviourBlackboardEntry InEntry)
		{
			if (m_Entries.TryFind(out AIBehaviourBlackboardEntry _, out int index, e => e.BlackboardEntryKey == InBlackboardKey))
			{
				m_Entries[index] = InEntry;
			}
			else
			{
				m_Entries.Add(InEntry);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> If entry doesn't exist a new one is created and the new value set </summary>
		public void SetEntryValue<T, V>(string InBlackboardKey, in V InNewValue) where T : AIBehaviourBlackboardEntryKeyValue<V>, new()
		{
			if (!string.IsNullOrEmpty(InBlackboardKey))
			{
				T entry = GetOrCreateEntry<T>(InBlackboardKey);
				{
					entry.SetValue(InNewValue);
				}
			}
		}
		*/
		public bool TryGetEntry<T>(string InBlackboardKey, out T OutEntry) where T : AIBehaviourBlackboardEntry, new()
		{
			OutEntry = null;
			bool bResult = false;
			if (!string.IsNullOrEmpty(InBlackboardKey) && m_Entries.TryFind(out AIBehaviourBlackboardEntry OutValue, out int _, e => e.BlackboardEntryKey == InBlackboardKey) && OutValue is T)
			{
				if (OutValue is T converted)
				{
					OutEntry = converted;
					bResult = true;
				}
			}
			return bResult;
		}
    }
}

