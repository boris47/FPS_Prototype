
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		private	Entity				m_Target						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent(this.gameObject, ref this.m_Target, ESearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				this.m_Target.OnEvent_Killed += this.OnKill;

				this.m_OnCompletionCallback = onCompletionCallback;
				this.m_OnFailureCallback = onFailureCallback;
				motherTask.AddObjective( this );
			}

			return this.m_IsInitialized;
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
		/// <summary> Set as current active to true and add indicator </summary>
		protected		override	void		ActivateInternal()
		{
			UIManager.Indicators.EnableIndicator(this.gameObject, EIndicatorType.TARGET_TO_KILL, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		/// <summary> Set as current active to false and remove indicator </summary>
		protected		override	void		DeactivateInternal()
		{
			UIManager.Indicators.DisableIndicator(this.gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill( Entity entityKilled )
		{
			this.Deactivate();

			this.OnObjectiveCompleted();
		}

	}

}
