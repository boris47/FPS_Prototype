
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
			if (this.m_LocalQuests.Count > 0 )
			{
				foreach( IQuest q in this.m_LocalQuests )
				{
					q.Initialize( this, this.OnQuestCompleted );
				}

				this.m_LocalQuests[0].Activate();
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
			if ( completedQuest.Scope != EQuestScope.LOCAL )
				return;

			bool bAreQuestsCompleted = this.m_LocalQuests.TrueForAll( ( Quest q ) => { return q.IsCompleted == true; } );
			if ( bAreQuestsCompleted )
			{
				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed All quests" );
			}
			else
			{
				int nextIndex = (this.m_LocalQuests.IndexOf( completedQuest as Quest ) + 1 );
				if ( nextIndex < this.m_LocalQuests.Count )
				{
					IQuest nextQuest = this.m_LocalQuests[ nextIndex ];
					nextQuest.Activate();
				}
			}

			IObjective objective = null;
			this.GetObjectiveByID( "LocationToReach", ref objective );
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestStatus ( Interface )
		EQuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if (this.m_LocalQuests.Count > questIndex )
				return EQuestStatus.NONE;

			IQuest quest = this.m_LocalQuests[ (int)questIndex ];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddQuest ( Interface )
		bool IQuestManager.AddQuest( IQuest newQuest, bool activateNow )
		{
			if ( newQuest == null )
				return false;

			if ( newQuest.Status == EQuestStatus.NONE )
				return false;

			Quest quest = newQuest as Quest;
			if (this.m_LocalQuests.Contains( quest ) == false )
				return false;

			this.m_LocalQuests.Add( quest );
			newQuest.Initialize( this, this.OnQuestCompleted );
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
			return this.m_LocalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetQuestSope ( Interface )
		EQuestScope IQuestManager.GetQuestSope( uint questIndex )
		{
			if (this.m_LocalQuests.Count > questIndex )
				return EQuestScope.NONE;

			IQuest nextQuest = this.m_LocalQuests[ (int)questIndex ];
			return nextQuest.Scope;
		}



		public	bool	GetTaskByID( string ID, ref ITask task )
		{
			bool result = false;

			Task[] allTasks = FindObjectsOfType<Task>();

			int index = System.Array.FindIndex( allTasks, delegate( ITask o ) { return o.ID == ID; } );
			if ( index > -1 )
			{
				task = allTasks[ index ];
				result = true;
			}

			return result;
		}


		public	bool	GetObjectiveByID( string ID, ref IObjective objective )
		{
			bool result = false;

			Objective_Base[] allObjectives = FindObjectsOfType<Objective_Base>();

			int index = System.Array.FindIndex( allObjectives, delegate( IObjective o ) { return o.ID == ID; } );
			if ( index > -1 )
			{
				objective = allObjectives[ index ];
				result = true;
			}

			return result;
		}
	}

}