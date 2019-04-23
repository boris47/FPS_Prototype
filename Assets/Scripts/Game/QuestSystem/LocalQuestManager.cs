
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	

	public class LocalQuestManager : MonoBehaviour, IQuestManager {

		private	static	IQuestManager m_Instance = null;
		public	static	IQuestManager Instance
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
//				t.AddToQuest( this );
				q.RegisterOnCompletion( OnQuestCompleted );
			}
			m_LocalQuests[0].Activate();
//			LocalQuestManager.Instance.AddQuest( this );

			/*
			Quest[] alreadyAssignedQuests = GetComponentsInChildren<Quest>();
			if ( alreadyAssignedQuests.Length > 0 )
			{
				foreach( IQuest q in alreadyAssignedQuests )
				{
					
				}

				m_LocalQuests.AddRange( alreadyAssignedQuests );
				m_LocalQuests[0].Activate();
			}
			*/
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

			bool bAreQuestsCompleted = true;
			foreach( IQuest q in m_LocalQuests )
			{
				bAreQuestsCompleted &= q.IsCompleted;
			}

			if ( bAreQuestsCompleted )
			{
				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed All quests" );
			}
			else
			{
				int index = m_LocalQuests.IndexOf( completedQuest as Quest );
				int nextIndex = ++index;
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
		bool IQuestManager.AddQuest( IQuest newQuest )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == QuestStatus.NONE )
				return false;

			if ( m_LocalQuests.Contains( newQuest as Quest ) == false )
				return false;

//			m_LocalQuests.Add( newQuest as Quest );
			newQuest.RegisterOnCompletion( OnQuestCompleted );
			newQuest.Activate();
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

		/*
		//////////////////////////////////////////////////////////////////////////
		// Update ( Interface )
		void IQuestManager.Update()
		{
			for ( int i = 0; i < m_LocalQuests.Count; i++ )
			{
				IQuest quest = m_LocalQuests[i];
				quest.Update();
			}
		}
		*/
	}

}