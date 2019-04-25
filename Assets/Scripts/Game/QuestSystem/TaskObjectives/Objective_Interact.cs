
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base {
		
		private	Interactable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public		override	bool		Initialize()
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;

			m_Interactable = GetComponent<Interactable>();
			if ( m_Interactable )
			{
				m_Interactable.CanInteract = true;
				m_Interactable.OnInteractionCallback += OnInteraction;
				result = true;
			}

			return result;
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
			UI.Instance.Indicators.EnableIndicator( m_Interactable.gameObject, IndicatorType.OBJECT_TO_INTERACT );

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
			if ( m_IsCompleted == true )
				return;

			Deactivate();
			
			OnObjectiveCompleted();
		}

	}

}