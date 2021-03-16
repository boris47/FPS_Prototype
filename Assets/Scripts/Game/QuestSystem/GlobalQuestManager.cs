using UnityEngine;

namespace QuestSystem
{
	using System.Collections.Generic;

	public interface IQuestManager
	{
		EQuestStatus	GetQuestStatus	(uint questIndex);

		bool			AddQuest		(Quest newQuest, bool activateNow);

		int				GetQuestCount	();

		EQuestScope		GetQuestScope	(uint questIndex);
	}

	public sealed class GlobalQuestManager : OnDemandSingleton<GlobalQuestManager>, IQuestManager
	{
	//	private		static	bool				m_ShowDebugInfo					= false;
	//	public		static	bool				ShowDebugInfo					=> m_ShowDebugInfo;

	//	private		static	IQuestManager		m_Instance						= null;
	//	public		static	IQuestManager		Instance						=> m_Instance;

		private				List<Quest>			m_GlobalQuests					= new List<Quest>();


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			// Already assigned
			foreach (Quest q in m_GlobalQuests)
			{
				q.Initialize(OnQuestCompleted, null);
			}

			if (m_GlobalQuests.Count > 0)
			{
				m_GlobalQuests[0].Activate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnQuestCompleted(Quest completedQuest)
		{
			if (completedQuest.Scope != EQuestScope.LOCAL)
				return;

			bool bAreQuestsCompleted = m_GlobalQuests.TrueForAll(q => q.IsCompleted);
			if (bAreQuestsCompleted)
			{
		//		if (ShowDebugInfo)
				{
					Debug.Log("Completed All quests");
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		EQuestStatus IQuestManager.GetQuestStatus(uint questIndex)
		{
			if (m_GlobalQuests.Count > questIndex)
				return EQuestStatus.NONE;

			Quest quest = m_GlobalQuests[(int)questIndex];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
		bool IQuestManager.AddQuest(Quest newQuest, bool activateNow)
		{
			if (newQuest == null)
				return false;

			if (newQuest.Status == EQuestStatus.NONE)
				return false;

			if (m_GlobalQuests.Contains(newQuest as Quest) == true)
				return false;

			m_GlobalQuests.Add(newQuest as Quest);
			newQuest.Initialize(OnQuestCompleted, null);
			if (activateNow)
			{
				newQuest.Activate();
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		int IQuestManager.GetQuestCount()
		{
			return m_GlobalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		EQuestScope IQuestManager.GetQuestScope(uint questIndex)
		{
			if (m_GlobalQuests.Count > questIndex)
				return EQuestScope.NONE;

			Quest quest = m_GlobalQuests[(int)questIndex];
			return quest.Scope;
		}
	}
}