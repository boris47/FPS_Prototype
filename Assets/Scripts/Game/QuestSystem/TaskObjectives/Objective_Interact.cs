
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base {
		
		[SerializeField]
		protected	GameEvent	m_OnInteraction	= null;

		private	Interactable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		// Awake
	//	private void Awake()
	//	{
	//
	//	}

		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			m_Interactable = GetComponent<Interactable>();
			m_Interactable.CanInteract = true;
			m_Interactable.OnInteractionCallback += OnInteraction;

			m_IsCurrentlyActive = true;
			m_Signal.gameObject.SetActive( m_IsCurrentlyActive );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnInteraction
		public void	OnInteraction()
		{
			if ( m_IsCompleted == true )
				return;

			m_IsCurrentlyActive = false;
			m_Signal.gameObject.SetActive( m_IsCurrentlyActive );

			if ( m_OnInteraction != null && m_OnInteraction.GetPersistentEventCount() > 0 )
				m_OnInteraction.Invoke();
			
			OnObjectiveCompleted();
		}

	}

}