
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Entity))]
	public class Objective_Destroy : Objective_Base
	{
		[SerializeField]
		private	Entity				m_Target						= null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Target );
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
		protected override void ActivateInternal()
		{
			UIManager.Indicators.AddIndicator(gameObject, EIndicatorType.TARGET_TO_KILL, bMustBeClamped: true);
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		/// <summary> Set as current active to false and remove indicator </summary>
		protected override void DeactivateInternal()
		{
			UIManager.Indicators.RemoveIndicator(gameObject);
		}


		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill( Entity entityKilled )
		{
			Deactivate();

			OnObjectiveCompleted();
		}

	}

}
