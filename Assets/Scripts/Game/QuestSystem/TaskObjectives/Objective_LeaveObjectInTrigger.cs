
using UnityEngine;


namespace QuestSystem {

	[RequireComponent(typeof(Collider))]
	public class Objective_LeaveObjectInTrigger : Objective_Base
	{
		[SerializeField]
		private	Collider			m_ObjectThatTrigger				= null;

		private	Collider			m_Collider						= null;



		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Collider );
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
			if (m_ObjectThatTrigger.IsNotNull())
			{
				m_Collider.enabled = true;

				UIManager.Indicators.AddIndicator(m_Collider.gameObject, EIndicatorType.AREA_WHERE_PLACE_OBJECT, bMustBeClamped: true);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			if (m_ObjectThatTrigger.IsNotNull() )
			{
				m_Collider.enabled = false;

				UIManager.Indicators.RemoveIndicator(gameObject );
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
			if (m_ObjectiveState != EObjectiveState.ACTIVATED )
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
