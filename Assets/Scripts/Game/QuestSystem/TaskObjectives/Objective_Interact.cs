
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base {
		
		private	IInteractable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.SearchComponent(this.gameObject, out this.m_Interactable, ESearchContext.LOCAL );
			if ( bIsGoodResult )
			{
				this.m_Interactable.CanInteract = true;
				this.m_Interactable.OnInteractionCallback += this.OnInteraction;
				this.m_Interactable.OnRetroInteractionCallback += this.OnRetroInteraction;

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
		protected		override	void		ActivateInternal()
		{	
			UIManager.Indicators.EnableIndicator(this.m_Interactable.Collider.gameObject, EIndicatorType.OBJECT_TO_INTERACT, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			UIManager.Indicators.DisableIndicator(this.gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnInteraction
		private void	OnInteraction()
		{
			this.Deactivate();

			this.OnObjectiveCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnRetroInteraction
		private	void	OnRetroInteraction()
		{
			// Require dependencies to be completed
			if (this.m_Dependencies.Count > 0 && this.m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
			{
				// Our depenencies ask for this objective to be completed, so we are goint to deaticate them
				this.m_Dependencies.ForEach( d => d.Deactivate() );

				// and activate again this
				this.Activate();

				this.m_IsCompleted = false;
			}
		}

	}

}