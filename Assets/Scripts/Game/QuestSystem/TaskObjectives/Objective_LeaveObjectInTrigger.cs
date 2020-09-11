
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
			if (this.m_ObjectThatTrigger.IsNotNull() )
			{
				this.m_Collider.enabled = true;

				UIManager.Indicators.EnableIndicator(this.m_Collider.gameObject, EIndicatorType.AREA_WHERE_PLACE_OBJECT, bMustBeClamped: true );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			if (this.m_ObjectThatTrigger.IsNotNull() )
			{
				this.m_Collider.enabled = false;

				UIManager.Indicators.DisableIndicator(this.gameObject );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// SetObjectToTrigger
		public	void	SetObjectToTriggerWith( Collider objCollider )
		{
			if ( objCollider && objCollider.isTrigger == false )
			{
				this.m_ObjectThatTrigger = objCollider;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTriggerEnter
		private void OnTriggerEnter( Collider other )
		{
			if (this.m_ObjectiveState != EObjectiveState.ACTIVATED )
				return;

			if ( other.GetInstanceID() != this.m_ObjectThatTrigger.GetInstanceID() )
				return;

			this.Deactivate();

			// Require dependencies to be completed
			int dependencyIndex = this.m_Dependencies.Count > 0 ? this.m_Dependencies.FindLastIndex( o => o.IsCompleted == false ) : -1;
			if ( dependencyIndex > -1 )
			{
				this.m_Dependencies[dependencyIndex].Activate();
				return;
			}

			this.OnObjectiveCompleted();
		}

	}

}
