
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base {
		
		private	IInteractable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize()
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			m_Interactable = GetComponent<IInteractable>();
			m_Interactable.CanInteract = true;
			m_Interactable.OnInteractionCallback += OnInteraction;
			m_Interactable.OnRetroInteractionCallback += OnRetroInteraction;

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
		// Activate ( IObjective )
		public		override	void		Activate()
		{	
			UI.Instance.Indicators.EnableIndicator( m_Interactable.Collider.gameObject, IndicatorType.OBJECT_TO_INTERACT );

			m_IsCurrentlyActive = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		override	void		Deactivate()
		{
			m_IsCurrentlyActive = false;

			UI.Instance.Indicators.DisableIndicator( gameObject );
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
			if ( m_Dependencies.Count > 0 && m_Dependencies.FindIndex( o => o.IsCompleted == false ) > -1 )
			{
				Activate();

				m_IsCompleted = false;
			}
		}

	}

}