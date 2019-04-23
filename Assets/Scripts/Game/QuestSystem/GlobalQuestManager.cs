
using UnityEngine;


namespace QuestSystem {

	using System.Collections.Generic;

	public interface IQuestManager {

		QuestStatus		GetQuestStatus	( uint questIndex );

		bool			AddQuest		( IQuest newQuest );

		int				GetQuestCount	();

		QuestScope		GetQuestSope	( uint questIndex );

	}

	public class GlobalQuestManager : MonoBehaviour, IQuestManager {

		public	static	bool				ShowDebugInfo					= false;

		public	static GlobalQuestManager	Instance						= null;

		private	LocalQuestManager			m_currentLocalQuestMnanager		= null;
		public	LocalQuestManager			CurrentLocalQuestManager
		{
			get { return m_currentLocalQuestMnanager; }
		}


		private	List<Quest>		m_GlobalQuests			= new List<Quest>();

		private void Awake()
		{
			Database.Section debugInfosSection = null;
			if ( GameManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
			{
				ShowDebugInfo = debugInfosSection.AsBool( "Quests", false );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnQuestCompleted
		private	void	OnQuestCompleted( IQuest completedQuest )
		{
			if ( completedQuest.Scope != QuestScope.LOCAL )
				return;

			bool bAreQuestsCompleted = true;
			foreach( IQuest q in m_GlobalQuests )
			{
				bAreQuestsCompleted &= q.IsCompleted;
			}

			if ( bAreQuestsCompleted )
			{
				if ( ShowDebugInfo )
					Debug.Log( "Completed All quests" );
			}
		}

		//////////////////////////////////////////////////////////////////////////
		// GetQuestStatus ( Interface )
		QuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if ( m_GlobalQuests.Count > questIndex )
				return QuestStatus.NONE;

			IQuest quest = m_GlobalQuests[ (int)questIndex ];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddQuest ( Interface )
		bool IQuestManager.AddQuest( IQuest newQuest )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == QuestStatus.NONE )
				return false;

			if ( m_GlobalQuests.Contains( newQuest as Quest ) == true )
				return false;

			m_GlobalQuests.Add( newQuest as Quest );
			newQuest.Activate();
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestCount ( Interface )
		int IQuestManager.GetQuestCount()
		{
			return m_GlobalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestSope ( Interface )
		QuestScope IQuestManager.GetQuestSope( uint questIndex )
		{
			if ( m_GlobalQuests.Count > questIndex )
				return QuestScope.NONE;

			IQuest quest = m_GlobalQuests[ (int)questIndex ];
			return quest.Scope;
		}

		/*
		//////////////////////////////////////////////////////////////////////////
		// Update ( Interface )
		void IQuestManager.Update()
		{
			for ( int i = 0; i < m_GlobalQuests.Count; i++ )
			{
				IQuest quest = m_GlobalQuests[i];
				quest.Update();
			}
		}
		*/
	}

}