using UnityEngine;
using System.Collections;

namespace QuestSystem {

	public enum QuestState {
		DISABLED,
		AVAIABLE,
		ENABLED,
		COMPLETED
	}

	public class Quest : MonoBehaviour {


		[SerializeField]
		private	Task[]		m_Tasks				= null;


		private	Task		m_CurrentTask		= null;
		private	int			m_CurrentTaskIdx	= 0;

		public	QuestState QuestState
		{
			get;
			private set;
		}


		private void Start()
		{
			if ( ( m_Tasks == null || m_Tasks.Length == 0 ) )
			{
				print( "No task found" );
				return;
			}

			QuestState = QuestState.DISABLED;

			foreach ( Task task in m_Tasks )
			{
				if ( task == null )
					continue;

				( task as ITask_Setter ).RelatedQuest = this;
			}

			m_CurrentTask = m_Tasks[0];
		}


		private Task	GetNextValidTask()
		{
			while( m_Tasks[ m_CurrentTaskIdx ] == null )
				m_CurrentTaskIdx ++;

			return m_Tasks[ m_CurrentTaskIdx ];
		}


		public void	NextTask()
		{
			if ( m_CurrentTask.Completed )
			{

				m_CurrentTaskIdx ++;

				if ( m_CurrentTaskIdx == m_Tasks.Length )
				{
					
				}

				m_CurrentTask = GetNextValidTask();
			}
		}

	}

}