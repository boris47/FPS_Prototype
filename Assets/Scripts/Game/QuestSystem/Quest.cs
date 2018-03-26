
using UnityEngine;

namespace QuestSystem {

	public class Quest : MonoBehaviour {
		
		public	bool				Completed				{ get; private set;	}

		[SerializeField]
		private	GameEvent			m_OnCompletion			= null;

//		[SerializeField]
		public	Task[]				Tasks
		{
			get; private set;
		}
		



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			Tasks = GetComponentsInChildren<Task>();
		}


		//////////////////////////////////////////////////////////////////////////
		// CheckQuestStatus
		public void	UpdateStatus()
		{
			this.Completed = true;
			foreach( Task task in Tasks )
			{
				if ( task.Completed == false )
				{
					task.Activate();
///					print( "-- Next Task is " + task.name );
					this.Completed = false;
					return;
				}
			}

			if ( this.Completed )
			{
				print( "Completed quest " + name );
				if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
					m_OnCompletion.Invoke();
				GlobalQuestManager.Instance.CurrentLocalQuestManager.UpdateStatus();

			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if ( Tasks == null || Tasks.Length == 0 )
			{
				return false;
			}
///			print( name + " quest activated" );
			Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
