
using UnityEngine;

namespace QuestSystem {

	public class Task : MonoBehaviour {
		

		public bool					Completed				{ get; private set; }

		[SerializeField]
		private	TaskObjective[]		m_TaskObjectives		= null;

		private Quest				RelatedQuest			{ get; set; }
		



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{

			foreach ( TaskObjective o in m_TaskObjectives )
				o.RelatedTask = this;

			RelatedQuest = transform.parent.GetComponent<Quest>();
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateStatus
		public void	UpdateStatus()
		{
			this.Completed = true;
			foreach( TaskObjective objective in m_TaskObjectives )
			{	
				if ( objective.Completed == false )
				{
					objective.CanInteract = true;
					print( "-- -- Next TaskObjective is " + objective.name );
					this.Completed = false;
					return;
				}
			}

			if ( this.Completed == true )
			{
				print( "Completed task " + name );
				RelatedQuest.UpdateStatus();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if ( m_TaskObjectives == null || m_TaskObjectives.Length == 0 )
			{
				return false;
			}

			// Activate first objective
			m_TaskObjectives[ 0 ].CanInteract = true;

			return true;
		}


	}

}
