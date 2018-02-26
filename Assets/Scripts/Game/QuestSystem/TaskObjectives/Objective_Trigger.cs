
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		[SerializeField]
		private GameEvent			m_OnTriggerEnter				= null;

		[SerializeField]
		private	bool				m_OneTimeTrigger				= false;

		private	Collider			m_Collider						= null;



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Collider = GetComponent<Collider>();
			m_Collider.isTrigger = true;
			m_Collider.enabled = false;
		}


		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			m_Collider.enabled = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( other.GetComponent<Player>() == null )
				return;

			// One time trigger
			if ( m_OneTimeTrigger == true )
				m_Collider.enabled = false;

			if ( m_OnTriggerEnter != null && m_OnTriggerEnter.GetPersistentEventCount() > 0 )
				m_OnTriggerEnter.Invoke();

			Completed = true;
			RelatedTask.UpdateStatus();
		}

	}

}
