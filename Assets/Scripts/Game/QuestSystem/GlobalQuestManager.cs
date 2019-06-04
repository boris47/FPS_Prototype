
using UnityEngine;


namespace QuestSystem {

	using System.Collections.Generic;

	public interface IQuestManager {

		QuestStatus		GetQuestStatus	( uint questIndex );

		bool			AddQuest		( IQuest newQuest, bool activateNow );

		int				GetQuestCount	();

		QuestScope		GetQuestSope	( uint questIndex );

	}

	public class GlobalQuestManager : MonoBehaviour, IQuestManager {

		private		static	bool				m_ShowDebugInfo					= false;
		public		static	bool				ShowDebugInfo
		{
			get { return m_ShowDebugInfo; }
		}
		
		private		static IQuestManager		m_Instance						= null;
		public		static	IQuestManager		Instance
		{
			get { return m_Instance; }
		}
		
		private	List<Quest>					m_GlobalQuests					= new List<Quest>();



		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void Awake()
		{
			m_Instance = this;
			Database.Section debugInfosSection = null;
			if ( GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
			{
				m_ShowDebugInfo = debugInfosSection.AsBool( "Quests", false );
			}

			// Already assigned
			foreach( IQuest q in m_GlobalQuests )
			{
				q.Initialize(this, OnQuestCompleted );
			}

			if ( m_GlobalQuests.Count > 0 )
			{
				m_GlobalQuests[0].Activate();
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
		bool IQuestManager.AddQuest( IQuest newQuest, bool activateNow )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == QuestStatus.NONE )
				return false;

			if ( m_GlobalQuests.Contains( newQuest as Quest ) == true )
				return false;

			m_GlobalQuests.Add( newQuest as Quest );
			newQuest.Initialize( this, OnQuestCompleted );
			if ( activateNow )
			{
				newQuest.Activate();
			}
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
	}

}