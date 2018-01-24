
using UnityEngine;

namespace QuestSystem {

	public class LocalQuestMnanager : MonoBehaviour {

		public Quest		currentActiveQuest	= null;

		private	Quest[]		m_Quests			= null;


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Quests = FindObjectsOfType<Quest>();
		}


		//////////////////////////////////////////////////////////////////////////
		// EnableQuest ( string )
		public	void	EnableQuest( string questName )
		{
			EnableQuest( System.Array.Find( m_Quests, q => q.name == questName ) );
		}

		//////////////////////////////////////////////////////////////////////////
		// EnableQuest ( Quest )
		public	void	EnableQuest( Quest quest )
		{
			if ( quest != null )
			{
				currentActiveQuest = quest;
				quest.currentTask.Interactable.CanInteract = true;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		public void	OnTaskCompleted( TaskObjective objective )
		{
			foreach ( Quest quest in m_Quests )
			{
				Task task = System.Array.Find( quest.Tasks, t => t.Interactable == objective );
				if ( task != null )
				{
					print( "Completed task " + task.name );
					task.Completed = true;
					quest.UpdateQuest( task );

					if ( quest.Completed && quest.NextQuest != null )
					{
						currentActiveQuest = quest.NextQuest;
						currentActiveQuest.currentTask.Interactable.CanInteract = true;
					}
				}
			}
		}

		public	void	Print()
		{
			print( "Now u are a Guardion!" );
		}

	}

}