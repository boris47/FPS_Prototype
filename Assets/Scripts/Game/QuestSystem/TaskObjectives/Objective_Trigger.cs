
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		[SerializeField]
		private	Collider			m_ObjectThatTrigger				= null;

		[SerializeField]
		private GameEvent			m_OnTriggerEnter				= new GameEvent();

		[SerializeField]
		private	bool				m_OneTimeTrigger				= false;

		private	Collider			m_Collider						= null;



		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			if ( m_ObjectThatTrigger == null )
			{
				gameObject.SetActive( false );
				return;
			}

			m_Collider = GetComponent<Collider>();
			m_Collider.isTrigger = true;
			m_Collider.enabled = false;
		}


		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Activate()
		{
			base.Activate();

			m_Collider.enabled = true;
			m_IsCurrentlyActive = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_IsCurrentlyActive == false )
				return;

			if ( other.GetInstanceID() != m_ObjectThatTrigger.GetInstanceID() )
				return;

			// One time trigger
			if ( m_OneTimeTrigger == true )
				m_Collider.enabled = false;

			if ( m_OnTriggerEnter != null && m_OnTriggerEnter.GetPersistentEventCount() > 0 )
				m_OnTriggerEnter.Invoke();

			m_IsCurrentlyActive = false;

			OnObjectiveCompleted();
		}

	}

}
