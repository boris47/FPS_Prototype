
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_Trigger : Objective_Base {

		private	Collider			m_Collider						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent(this.gameObject, out this.m_Collider, ESearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				this.m_Collider.isTrigger = true;
				this.m_Collider.enabled = false;

				this.m_OnCompletionCallback = onCompletionCallback;
				this.m_OnFailureCallback = onFailureCallback;
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
			this.m_Collider.enabled = true;

			UIManager.Indicators.EnableIndicator(this.gameObject, EIndicatorType.AREA_TO_REACH, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			this.m_Collider.enabled = false;

			UIManager.Indicators.DisableIndicator(this.gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if (this.m_ObjectiveState != EObjectiveState.ACTIVATED )
				return;

			if ( other.GetInstanceID() != Player.Entity.PhysicCollider.GetInstanceID() )
				return;

			// Require dependencies to be completed
			if (this.m_Dependencies.Count > 0 && this.m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
				return;

			this.Deactivate();

			this.OnObjectiveCompleted();
		}

	}

}
