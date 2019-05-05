
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		private	Collider			m_Collider						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent( gameObject, ref m_Collider, SearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				m_Collider.isTrigger = true;
				m_Collider.enabled = false;
				
				m_OnCompletionCallback = onCompletionCallback;
				m_OnFailureCallback = onFailureCallback;
				motherTask.AddObjective( this );
			}
			
			return bIsGoodResult;
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
		// OnSave
		public override void OnSave( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public override void OnLoad( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate ( IObjective )
		protected		override	void		ActivateInternal()
		{
			m_Collider.enabled = true;

			UI.Instance.Indicators.EnableIndicator( gameObject, IndicatorType.AREA_TO_REACH );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			m_Collider.enabled = false;

			UI.Instance.Indicators.DisableIndicator( gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_ObjectiveState != ObjectiveState.ACTIVATED )
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
