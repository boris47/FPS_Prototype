
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	

	public sealed class LocalQuestManager : MonoBehaviour, IQuestManager {

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
			if (m_LocalQuests.Count > 0 )
			{
				foreach( Quest q in m_LocalQuests )
				{
					q.Initialize( OnQuestCompleted, null );
				}

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
		private	void	OnQuestCompleted( Quest completedQuest )
		{
			if ( completedQuest.Scope != EQuestScope.LOCAL )
				return;

			bool bAreQuestsCompleted = m_LocalQuests.TrueForAll( ( Quest q ) => { return q.IsCompleted == true; } );
			if ( bAreQuestsCompleted )
			{
				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed All quests" );
			}
			else
			{
				int nextIndex = (m_LocalQuests.IndexOf( completedQuest as Quest ) + 1 );
				if ( nextIndex < m_LocalQuests.Count )
				{
					Quest nextQuest = m_LocalQuests[ nextIndex ];
					nextQuest.Activate();
				}
			}

			Objective_Base objective = null;
			GetObjectiveByID( "LocationToReach", ref objective );
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestStatus ( Interface )
		EQuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if (m_LocalQuests.Count > questIndex )
				return EQuestStatus.NONE;

			Quest quest = m_LocalQuests[ (int)questIndex ];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddQuest ( Interface )
		bool IQuestManager.AddQuest( Quest newQuest, bool activateNow )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == EQuestStatus.NONE )
				return false;

			Quest quest = newQuest as Quest;
			if (m_LocalQuests.Contains( quest ) == false )
				return false;

			m_LocalQuests.Add( quest );
			newQuest.Initialize( OnQuestCompleted, null );
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
		EQuestScope IQuestManager.GetQuestScope( uint questIndex )
		{
			if (m_LocalQuests.Count > questIndex )
				return EQuestScope.NONE;

			Quest nextQuest = m_LocalQuests[ (int)questIndex ];
			return nextQuest.Scope;
		}



		public	bool	GetTaskByID( string ID, ref Task task )
		{
			bool result = false;

			Task[] allTasks = FindObjectsOfType<Task>();

			int index = System.Array.FindIndex( allTasks, t => t.ID == ID );
			if ( index > -1 )
			{
				task = allTasks[ index ];
				result = true;
			}

			return result;
		}


		public	bool	GetObjectiveByID( string ID, ref Objective_Base objective )
		{
			bool result = false;

			Objective_Base[] allObjectives = FindObjectsOfType<Objective_Base>();

			int index = System.Array.FindIndex( allObjectives, o => o.ID == ID );
			if ( index > -1 )
			{
				objective = allObjectives[ index ];
				result = true;
			}

			return result;
		}
	}

}