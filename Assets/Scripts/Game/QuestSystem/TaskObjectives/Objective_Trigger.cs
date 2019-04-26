
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		private	Collider			m_Collider						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize()
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			m_Collider = GetComponent<Collider>();
			m_Collider.isTrigger = true;
			m_Collider.enabled = false;

			return m_IsInitialized;
		}


		//////////////////////////////////////////////////////////////////////////
		// ReInit ( IStateDefiner )
		public		override	bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Finalize ( IStateDefiner )
		public		override	bool		Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate ( IObjective )
		public		override	void		Activate()
		{
			m_Collider.enabled = true;

			m_IsCurrentlyActive = true;

			UI.Instance.Indicators.EnableIndicator( m_Collider.gameObject, IndicatorType.AREA_TO_REACH );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		override	void		Deactivate()
		{
			m_Collider.enabled = false;

			m_IsCurrentlyActive = false;

			UI.Instance.Indicators.DisableIndicator( gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_IsCurrentlyActive == false )
				return;

			if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
				return;

			// Require dependencies to be completed
			if ( m_Dependencies.Count > 0 && m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
				return;

			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
