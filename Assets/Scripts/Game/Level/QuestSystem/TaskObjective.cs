
using UnityEngine;

namespace QuestSystem {

	public class TaskObjective : Interactable {

		public Task					RelatedTask			{ private get; set; }
		public	bool				Completed			{ get; private set; }

		[SerializeField]
		private GameEvent			m_OnInteraction		= null;
		



		//////////////////////////////////////////////////////////////////////////
		// OnInteraction ( Override )
		public override void	OnInteraction()
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