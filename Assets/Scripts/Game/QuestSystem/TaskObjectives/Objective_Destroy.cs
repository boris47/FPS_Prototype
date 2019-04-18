
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		[SerializeField]
		private GameEvent			m_OnDestroy						= null;

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
			m_IsCurrentlyActive = true;
			m_Signal.gameObject.SetActive( m_IsCurrentlyActive );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void OnKill()
		{
			if ( m_OnDestroy != null && m_OnDestroy.GetPersistentEventCount() > 0 )
				m_OnDestroy.Invoke();

			m_Signal.SetParent( null );
			Destroy( m_Signal.gameObject );

			m_IsCurrentlyActive = false;
			m_Signal.gameObject.SetActive( m_IsCurrentlyActive );

			OnObjectiveCompleted();
		}

	}

}
