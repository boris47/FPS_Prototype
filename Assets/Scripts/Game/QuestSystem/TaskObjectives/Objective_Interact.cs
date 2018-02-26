
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base, IInteractable {

		[SerializeField]
		private GameEvent			m_OnInteraction					= null;

		private bool                m_Interactable                  = false;
		bool						IInteractable.CanInteract		{ get { return m_Interactable; } }



		//////////////////////////////////////////////////////////////////////////
		// Enable ( Override )
		public override void Enable()
		{
			m_Interactable = true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnInteraction
		public void	OnInteraction()
		{
			if ( Completed == true )
				return;

			if ( m_OnInteraction != null && m_OnInteraction.GetPersistentEventCount() > 0 )
				m_OnInteraction.Invoke();
			
///			print( "Task Objective completed: " + name );

			Completed = true;
			RelatedTask.UpdateStatus();
		}

	}

}