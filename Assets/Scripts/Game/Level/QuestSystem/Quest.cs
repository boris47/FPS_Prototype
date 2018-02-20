
using UnityEngine;

namespace QuestSystem {

	public class Quest : MonoBehaviour {
		
		public	bool				Completed				{ get; private set;	}

//		[SerializeField]
		private	Task[]				m_Tasks					= null;
		public	Task[]				Tasks
		{
			get { return m_Tasks; }
		}




		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Tasks = GetComponentsInChildren<Task>();
		}


		//////////////////////////////////////////////////////////////////////////
		// CheckQuestStatus
		public void	UpdateStatus()
		{
			this.Completed = true;
			foreach( Task task in m_Tasks )
			{
				if ( task.Completed == false )
				{
					task.Activate();
					print( "-- Next Task is " + task.name );
					this.Completed = false;
					return;
				}
			}

			if ( this.Completed )
			{
				print( "Completed quest " + name );
				GlobalQuestManager.Instance.CurrentLocalQuestManager.UpdateStatus();

			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if ( m_Tasks == null || m_Tasks.Length == 0 )
			{
				return false;
			}

			m_Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
