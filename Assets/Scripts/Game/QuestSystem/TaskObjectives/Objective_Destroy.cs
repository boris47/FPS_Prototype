
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider), typeof(Entity))]
	public class Objective_Destroy : Objective_Base {

		private	Entity				m_Target						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent( gameObject, ref m_Target, SearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				m_Target.OnEvent_Killed += OnKill;

				m_OnCompletionCallback = onCompletionCallback;
				m_OnFailureCallback = onFailureCallback;
				motherTask.AddObjective( this );
			}

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
			UI.Instance.Indicators.EnableIndicator( gameObject, IndicatorType.TARGET_TO_KILL );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		/// <summary> Set as current active to false and remove indicator </summary>
		protected		override	void		DeactivateInternal()
		{
			UI.Instance.Indicators.DisableIndicator( gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill()
		{
			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
