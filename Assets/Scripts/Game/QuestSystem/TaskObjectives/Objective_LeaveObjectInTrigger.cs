
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_LeaveObjectInTrigger : Objective_Base {

		[SerializeField]
		private	Collider			m_ObjectThatTrigger				= null;

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
			if ( m_ObjectThatTrigger.IsNotNull() )
			{
				m_Collider.enabled = true;

				UI.Instance.Indicators.EnableIndicator( m_Collider.gameObject, IndicatorType.AREA_WHERE_PLACE_OBJECT );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			if ( m_ObjectThatTrigger.IsNotNull() )
			{
				m_Collider.enabled = false;

				UI.Instance.Indicators.DisableIndicator( gameObject );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// SetObjectToTrigger
		public	void	SetObjectToTriggerWith( Collider objCollider )
		{
			if ( objCollider && objCollider.isTrigger == false )
			{
				m_ObjectThatTrigger = objCollider;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if ( m_ObjectiveState != ObjectiveState.ACTIVATED )
				return;

			if ( other.GetInstanceID() != m_ObjectThatTrigger.GetInstanceID() )
				return;

			Deactivate();

			// Require dependencies to be completed
			int dependencyIndex = m_Dependencies.Count > 0 ? m_Dependencies.FindLastIndex( o => o.IsCompleted == false ) : -1;
			if ( dependencyIndex > -1 )
			{
				m_Dependencies[dependencyIndex].Activate();
				return;
			}

			OnObjectiveCompleted();
		}

	}

}
