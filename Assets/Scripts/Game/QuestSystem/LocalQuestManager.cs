
using UnityEngine;

namespace QuestSystem
{
	using System.Collections.Generic;


	public sealed class LocalQuestManager : MonoBehaviour, IQuestManager
	{
		private		static		IQuestManager		m_Instance					= null;
		public		static		IQuestManager		Instance					=> m_Instance;

		[SerializeField]
		private					List<Quest>			m_LocalQuests				= new List<Quest>();



		//////////////////////////////////////////////////////////////////////////
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
		private void OnDestroy()
		{
			m_Instance = null;
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	OnQuestCompleted(Quest completedQuest)
		{
			if (completedQuest.Scope == EQuestScope.LOCAL)
			{
				bool bAreQuestsCompleted = m_LocalQuests.TrueForAll(q => q.IsCompleted);
				if (bAreQuestsCompleted)
				{
					//if ( GlobalQuestManager.ShowDebugInfo )
					//	print( "Completed All quests" );
				}
				else
				{
					int nextIndex = (m_LocalQuests.IndexOf(completedQuest as Quest) + 1);
					if (nextIndex < m_LocalQuests.Count)
					{
						Quest nextQuest = m_LocalQuests[nextIndex];
						nextQuest.Activate();
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		EQuestStatus IQuestManager.GetQuestStatus( uint questIndex )
		{
			if (m_LocalQuests.Count > questIndex )
				return EQuestStatus.NONE;

			Quest quest = m_LocalQuests[ (int)questIndex ];
			return quest.Status;
		}


		//////////////////////////////////////////////////////////////////////////
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
		int IQuestManager.GetQuestCount()
		{
			return m_LocalQuests.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		EQuestScope IQuestManager.GetQuestScope( uint questIndex )
		{
			if (m_LocalQuests.Count > questIndex )
				return EQuestScope.NONE;

			Quest nextQuest = m_LocalQuests[ (int)questIndex ];
			return nextQuest.Scope;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool	GetTaskByID( string ID, out Task task )
		{
			task = null;

			// TODO store all task in a global list
			Task[] allTasks = FindObjectsOfType<Task>();
			int index = System.Array.FindIndex( allTasks, t => t.ID == ID );
			if ( index > -1 )
			{
				task = allTasks[ index ];
				return true;
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool	GetObjectiveByID( string ID, out Objective_Base objective )
		{
			objective = null;

			// TODO store all Objectives in a global list
			Objective_Base[] allObjectives = FindObjectsOfType<Objective_Base>();
			int index = System.Array.FindIndex( allObjectives, o => o.ID == ID );
			if ( index > -1 )
			{
				objective = allObjectives[ index ];
				return true;
			}
			return false;
		}
	}

}