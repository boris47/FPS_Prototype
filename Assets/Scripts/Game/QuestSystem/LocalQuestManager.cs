
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	

	public class LocalQuestManager : MonoBehaviour, IQuestManager {

		private		static IQuestManager		m_Instance						= null;
		public		static	IQuestManager		Instance
		{
			get { return m_Instance; }
		}

		[SerializeField]
		private	List<Quest>		m_LocalQuests			= new List<Quest>();
		



		//////////////////////////////////////////////////////////////////////////
		// Awake
		private void Awake()
		{
			m_Instance = this;

			// Already assigned
			foreach( IQuest q in m_LocalQuests )
			{
				q.Initialize( this, OnQuestCompleted );
			}
			if ( m_LocalQuests.Count > 0 )
			{
				m_LocalQuests[0].Activate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void OnDestroy()
		{
			m_Instance = null;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnQuestCompleted
		private	void	OnQuestCompleted( IQuest completedQuest )
		{
			if ( completedQuest.Scope != QuestScope.LOCAL )
				return;

			bool bAreQuestsCompleted = m_LocalQuests.TrueForAll( ( Quest q ) => { return q.IsCompleted == true; } );
			if ( bAreQuestsCompleted )
			{
				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed All quests" );
			}
			else
			{
				int nextIndex = ( m_LocalQuests.IndexOf( completedQuest as Quest ) + 1 );
				if ( nextIndex < m_LocalQuests.Count )
				{
					IQuest nextQuest = m_LocalQuests[ nextIndex ];
					nextQuest.Activate();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestStatus ( Interface )
		QuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if ( m_LocalQuests.Count > questIndex )
				return QuestStatus.NONE;

			IQuest quest = m_LocalQuests[ (int)questIndex ];
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

			Quest quest = newQuest as Quest;
			if ( m_LocalQuests.Contains( quest ) == false )
				return false;

			m_LocalQuests.Add( quest );
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
			return m_LocalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestSope ( Interface )
		QuestScope IQuestManager.GetQuestSope( uint questIndex )
		{
			if ( m_LocalQuests.Count > questIndex )
				return QuestScope.NONE;

			IQuest nextQuest = m_LocalQuests[ (int)questIndex ];
			return nextQuest.Scope;
		}
	}

}