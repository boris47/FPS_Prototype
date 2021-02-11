
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base
	{	
		[SerializeField]
		private	Interactable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool bIsGoodResult = Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Interactable );
			if ( bIsGoodResult )
			{
				m_Interactable.CanInteract = true;
				m_Interactable.OnInteractionCallback += OnInteraction;
				m_Interactable.OnRetroInteractionCallback += OnRetroInteraction;

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
		protected		override	void		ActivateInternal()
		{	
			UIManager.Indicators.EnableIndicator(m_Interactable.Collider.gameObject, EIndicatorType.OBJECT_TO_INTERACT, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			UIManager.Indicators.DisableIndicator(gameObject );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnInteraction
		private void	OnInteraction()
		{
			Deactivate();

			OnObjectiveCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnRetroInteraction
		private	void	OnRetroInteraction()
		{
			// Require dependencies to be completed
			if (m_Dependencies.Count > 0 && m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
			{
				// Our depenencies ask for this objective to be completed, so we are goint to deaticate them
				m_Dependencies.ForEach( d => d.Deactivate() );

				// and activate again this
				Activate();

				m_IsCompleted = false;
			}
		}

	}

}