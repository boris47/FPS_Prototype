
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		[SerializeField]
		private GameEvent			m_OnDestoyied					= null;

		private	Entity				m_Target						= null;


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			m_Target	= GetComponent<Entity>();
			m_Target.OnKilled += OnKill;
		}


		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			
		}

		//////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void OnKill()
		{
			if ( m_OnDestoyied != null && m_OnDestoyied.GetPersistentEventCount() > 0 )
				m_OnDestoyied.Invoke();

			Completed = true;
			RelatedTask.UpdateStatus();
		}

	}

}
