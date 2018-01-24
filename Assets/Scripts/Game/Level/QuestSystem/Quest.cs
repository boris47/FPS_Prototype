
using UnityEngine;

namespace QuestSystem {

	public class Quest : MonoBehaviour {
		
		public	Task		currentTask		= null;
		public	Quest		NextQuest		= null;

		[SerializeField]
		private	Task[]		m_Tasks			= null;
		public	Task[]		Tasks
		{
			get { return m_Tasks; }
		}

		public	bool	Completed
		{
			get;
			set;
		}


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			currentTask = m_Tasks[ 0 ];
			foreach ( Task task in m_Tasks )
			{
				task.RelatedQuest = this;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateQuest
		public void	UpdateQuest( Task lastCompletedTask )
		{
			if ( lastCompletedTask.NextTask != null )
			{
				lastCompletedTask.NextTask.Interactable.CanInteract = true;
				currentTask =  lastCompletedTask.NextTask;
			}

			Completed = true;
			foreach( Task task in m_Tasks )
			{
				if ( task.Completed == false )
				{
					Completed = false;
					return;
				}
			}

			if ( Completed )
				print( "Completed quest " + name );
		}
		
	}

}
