
using UnityEngine;

namespace QuestSystem {

	public class Task : MonoBehaviour {

		public bool							Completed				{ get; private set; }

		[SerializeField]
		private GameEvent					m_OnCompletion			= null;

		[SerializeField]
		private	Objective_Base[]			m_TaskObjectives		= null;

		private Quest						RelatedQuest			{ get; set; }
		

		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			RelatedQuest = transform.parent.GetComponent<Quest>();

			foreach ( Objective_Base o in m_TaskObjectives )
			{
				if ( o == null )
				{
					Debug.Log( "task " + name + ", related to quest " + RelatedQuest.name + " has invalid objective" );
				}
				else
				{
					o.RelatedTask = this;
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// UpdateStatus
		public void	UpdateStatus()
		{
			this.Completed = true;
			foreach( Objective_Base objective in m_TaskObjectives )
			{	
				if ( objective.Completed == false )
				{
					objective.Enable();
///					print( "-- -- Next TaskObjective is " + objective.name );
					this.Completed = false;
					return;
				}
			}

			if ( this.Completed == true )
			{
///				print( "Completed task " + name );
				if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
					m_OnCompletion.Invoke();
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
///			print( name + " task activated" );
			// Activate first objective
			m_TaskObjectives[ 0 ].Enable();
			return true;
		}


	}

}
